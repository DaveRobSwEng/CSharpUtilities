#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// DictionaryManager.cs
//  Implementation of the Class DictionaryManager
//
// Copyright (c) 2010 Sepura Plc
// All Rights reserved.
//
//  Original author: robinsond
//
// $Id:$
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Sepura.Utilities;
    using Wintellect.PowerCollections;

    /// <summary>
    /// Class responsible for managing the contents of a data dictionary
    /// The class loads signal and process definitions from the XML file when the file is loaded
    /// and then loads data element definitions from the XML file on demand.
    /// Loaded element definitions are cached so that they are only read once from the file, subsequent
    /// requests are supplied from the cache.
    /// Specification for the XML file format is given here:
    /// \\serv12\drp\prp10d\sw_project_management\work_packages\wp0846 Terminal Support for WIN TSE\Technical\Architecture\Data Dictionary Spec\TSE Data Dictionary Specification Issue 2_1.doc
    /// </summary>
    public class DictionaryManager
    {
        /// <summary>
        /// Gets the collection of Signals
        /// </summary>
        public ReadOnlyCollection<SignalDescription> Signals
        {
            get
            {
                return m_Signals;
            }
        }

        /// <summary>
        /// Gets the collection of Processes
        /// </summary>
        public ReadOnlyCollection<ProcessDescription> Processes
        {
            get
            {
                return m_Processes;
            }
        }

        /// <summary>
        /// Gets the Dictionary hash
        /// </summary>
        public string DictionaryHash
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the object has loaded a data dictionary file or not
        /// </summary>
        public bool IsLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the endianism of the system.
        /// </summary>
        /// <value>
        /// The endianism.
        /// </value>
        public static Endianism Endianism
        {
            get;
            set;
        }

        /// <summary>
        /// Follow typedefs until a concrete type is found
        /// </summary>
        /// <param name="theType"></param>
        /// <returns>Type definition aliased by theType</returns>
        public static TypeDefinition DereferenceTypeDef(TypeDefinition theType)
        {
            TypedefDefinition theTypeDef = theType as TypedefDefinition;
            while (theTypeDef != null)
            {
                theType = theTypeDef.AliasedType;
                theTypeDef = theType as TypedefDefinition;
            }

            return theType;
        }

        /// <summary>
        /// Initializes a new instance of the DictionaryManager class
        /// </summary>
        public DictionaryManager()
        {
            IsLoaded = false;

            m_TypeDefinitionsByName = new MultiDictionary<string, TypeDefinition>(false);

            // Add special built-in type 'String'
            StringDefinition theStringDefinition = new StringDefinition();
            m_TypeDefinitionsByName.Add(theStringDefinition.Name, theStringDefinition);
        }

        /// <summary>
        /// Load a data dictionary from an XML file
        /// </summary>
        /// <param name="fileName">File to load</param>
        public void Load(string fileName)
        {
            IsLoaded = false;
            if (!File.Exists(fileName))
            {
                throw new DataDictionaryException("File not found: {0}", fileName);
            }

            Load(new FileInfo(fileName));
        }

        /// <summary>
        /// Load a data dictionary from an XML file
        /// </summary>
        /// <param name="fileInfo">File to load</param>
        public void Load(FileSystemInfo fileInfo)
        {
            try
            {
                LoadInternal(fileInfo);
            }
            catch (System.Xml.XmlException ex)
            {
                m_TraceSource.TraceInformation("Load DD from {0} threw XmlException {1}s", fileInfo.FullName, ex.Message);
                throw new DataDictionaryException(ex.Message, ex);
            }
            catch (DataDictionaryException ex)
            {
                m_TraceSource.TraceInformation("Load DD from {0} threw DataDictionaryException {1}s", fileInfo.FullName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Internal method to load a data dictionary from XML file without trapping exceptions.
        /// Exceptions are caught in the public function to make the code a little tidier
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        private void LoadInternal(FileSystemInfo fileInfo)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Load the text into memory as a tree of XML nodes
            m_DataDictionaryXml = XElement.Load(fileInfo.FullName);

            // Dictionary Hash element is optional
            XElement hashElement = m_DataDictionaryXml.Descendants("Hash").FirstOrDefault();
            DictionaryHash = hashElement == null ? string.Empty : hashElement.Value;

            stopWatch.Stop();
            m_TraceSource.TraceInformation("Load DD from {0} took {1}s", fileInfo.FullName, stopWatch.Elapsed.TotalSeconds);

            stopWatch.Reset();
            stopWatch.Start();

            // Read the signal descriptions nodes from the tree
            // Sorting by signal name build a sequence of SignalDescription objects
            // each containing the name, value and type (corresponding data type)
            // of a signal
            // The final element of the list has no Type element so this has to be handled
            // in the where clause
            var signalDescriptions = from signal in m_DataDictionaryXml.Descendants("Signal")
                                     where signal.Element("Type") != null
                                     orderby signal.Element("Name").Value
                                     select new SignalDescription(signal.Element("Name").Value,
                                         (uint)Formatting.ParseInt(signal.Element("Value").Value),
                                         signal.Element("Type").Value);

            m_Signals = new ReadOnlyCollection<SignalDescription>(signalDescriptions.ToList());

            // Build the m_SignalDescriptions dictionary
            m_SignalDescriptionsByName.Clear();
            m_SignalDescriptionsById.Clear();
            foreach (var signalDescription in signalDescriptions)
            {
                m_SignalDescriptionsByName[signalDescription.Name] = signalDescription;
                m_SignalDescriptionsById[signalDescription.Value] = signalDescription;
            }

            stopWatch.Stop();
            m_TraceSource.TraceInformation("Reading signals took {0}s", stopWatch.Elapsed.TotalSeconds);

            stopWatch.Reset();
            stopWatch.Start();

            // Read the process descriptions nodes from the tree
            // Sorting by process name build a sequence of ProcessDescription objects
            // each containing the name, value and long name of a process
            // The final element of the list has no Type element so this has to be handled
            // in the where clause
            var processDescriptions = from process in m_DataDictionaryXml.Descendants("Process")
                                      orderby process.Element("Name").Value
                                      select new ProcessDescription(process.Element("Name").Value,
                                         (uint)Formatting.ParseInt(process.Element("Value").Value),
                                         process.Element("LongName").Value);

            m_Processes = new ReadOnlyCollection<ProcessDescription>(processDescriptions.ToList());

            // Build the m_ProcessDescriptions dictionary
            m_ProcessDescriptionsByName.Clear();
            m_ProcessDescriptionsById.Clear();
            foreach (var processDescription in processDescriptions)
            {
                m_ProcessDescriptionsByName[processDescription.Name] = processDescription;
                m_ProcessDescriptionsById[processDescription.Value] = processDescription;
            }

            stopWatch.Stop();
            m_TraceSource.TraceInformation("Reading processes took {0}s", stopWatch.Elapsed.TotalSeconds);

            IsLoaded = true;
        }

        /// <summary>
        /// Unloads this instance.
        /// Clears all stored information read from the XML file
        /// </summary>
        public void Unload()
        {
            IsLoaded = false;
            m_Signals = new ReadOnlyCollection<SignalDescription>(new SignalDescription[] { });
            m_Processes = new ReadOnlyCollection<ProcessDescription>(new ProcessDescription[] { });
            m_SignalDescriptionsByName.Clear();
            m_SignalDescriptionsById.Clear();
            m_ProcessDescriptionsByName.Clear();
            m_ProcessDescriptionsById.Clear();
            m_TypeDefinitionsByRefId.Clear();
            m_TypeDefinitionsByName.Clear();
        }

        /// <summary>
        /// Retrieves a signal description corresponding to the signal name
        /// </summary>
        /// <param name="signalName">Name of the signal</param>
        /// <returns>Signal description object or null if not found</returns>
        public SignalDescription GetSignalDescription(string signalName)
        {
            SignalDescription signalDescription = null;
            m_SignalDescriptionsByName.TryGetValue(signalName, out signalDescription);
            return signalDescription;
        }

        /// <summary>
        /// Retrieves a signal description corresponding to the signal name
        /// </summary>
        /// <param name="signalId">The signal id.</param>
        /// <returns>
        /// Signal description object or null if not found
        /// </returns>
        public SignalDescription GetSignalDescription(uint signalId)
        {
            SignalDescription signalDescription = null;
            m_SignalDescriptionsById.TryGetValue(signalId, out signalDescription);
            return signalDescription;
        }

        /// <summary>
        /// Retrieves a process description corresponding to the process name
        /// </summary>
        /// <param name="processName">Name of the process</param>
        /// <returns>Process description or null if not found</returns>
        public ProcessDescription GetProcessDescription(string processName)
        {
            ProcessDescription processDescription = null;
            m_ProcessDescriptionsByName.TryGetValue(processName, out processDescription);
            return processDescription;
        }

        /// <summary>
        /// Retrieves a process description corresponding to the process name
        /// </summary>
        /// <param name="processId">The process id.</param>
        /// <returns>
        /// Process description or null if not found
        /// </returns>
        public ProcessDescription GetProcessDescription(uint processId)
        {
            ProcessDescription processDescription = null;
            m_ProcessDescriptionsById.TryGetValue(processId, out processDescription);
            return processDescription;
        }

        /// <summary>
        /// Return an object describing the structure of the signal.
        /// This may be a structure definition or simply a reference to SIGSELECT
        /// </summary>
        /// <param name="signalName"></param>
        /// <returns>Type definition corresponding to the signal name</returns>
        public TypeDefinition GetSignalTypeDefinition(string signalName)
        {
            SignalDescription signalDescription = null;
            if (!m_SignalDescriptionsByName.TryGetValue(signalName, out signalDescription))
            {
                return null;
            }

            TypeDefinition signalType = GetElementType(signalDescription.TypeName, TypeId.StructType);
            if (signalType == null)
            {
                // Can't find a type definition that defines a struct so try the defaults
                signalType = GetElementType(signalDescription.TypeName);
            }

            return signalType;
        }

        /// <summary>
        /// Return an object describing the structure of the signal.
        /// This may be a structure definition or simply a reference to SIGSELECT
        /// </summary>
        /// <param name="signalId">The signal id.</param>
        /// <returns>
        /// Type definition corresponding to the signal name
        /// </returns>
        public TypeDefinition GetSignalTypeDefinition(uint signalId)
        {
            SignalDescription signalDescription = null;
            if (!m_SignalDescriptionsById.TryGetValue(signalId, out signalDescription))
            {
                return null;
            }

            TypeDefinition signalType = GetElementType(signalDescription.TypeName, TypeId.StructType);
            if (signalType == null)
            {
                // Can't find a type definition that defines a struct so try the defaults
                signalType = GetElementType(signalDescription.TypeName);
            }

            signalType = DictionaryManager.DereferenceTypeDef(signalType);

            return signalType;
        }

        /// <summary>
        /// Gets the TypeDefinition for the specified type node string
        /// </summary>
        /// <param name="typeNode">The string from the Type node. This may be:
        /// Integer identifying type reference
        /// [type] typename specifying something like struct TheStruct
        /// typename - implies either 'basename' or 'typedef' preceding the typename</param>
        /// <returns>TypeDefinition for the specified type node string</returns>
        public TypeDefinition GetElementType(string typeNode)
        {
            // Check first for built-in types - these take precedence over data-dictionary-defined types
            TypeDefinition theTypeDefinition = null;
            if (s_BuiltInTypes.TryGetValue (typeNode, out theTypeDefinition))
            {
                return theTypeDefinition;
            }

            // Check for type ref id
            int typeRefId;
            if (Formatting.TryGetInt(typeNode, out typeRefId))
            {
                return GetTypeDefinition(typeRefId);
            }

            // e.g. 'typedef unsigned long'
            // e.g. 'enum eTSE_L3'
            string[] typeElements = typeNode.Split(new char[] { ' ' });

            // First see if the first word is a keyword specifying the class of the type definition
            if (s_ObjectNamesToTypes.ContainsKey(typeElements[0]))
            {
                theTypeDefinition = GetElementType(typeNode.Substring(typeElements[0].Length + 1), s_ObjectNamesToTypes[typeElements[0]]);
            }
            else
            {
                // If not a keyword then try looking for a typedef and if that's unsuccessful try base type
                theTypeDefinition = GetElementType(typeNode, TypeId.TypeDefType);
                if (theTypeDefinition == null)
                {
                    theTypeDefinition = GetElementType(typeNode, TypeId.BaseType);
                }
            }

            if (theTypeDefinition == null)
            {
                throw new DataDictionaryException("Cannot find a type to match '{0}'", typeNode);
            }

            return theTypeDefinition;
        }

        /// <summary>
        /// Gets an unique id for an unnamed item
        /// </summary>
        /// <returns>A unique ID value</returns>
        public int GetUniqueId()
        {
            return ++m_UniqueId;
        }

        /// <summary>
        /// Returns an object describing the type of the named signal element
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="elementType">Type of the element.</param>
        /// <returns>
        /// Type definition corresponding to the signal element name or null if not found
        /// </returns>
        private TypeDefinition GetElementType(string elementName, TypeId elementType)
        {
            // First look in the collection of known types
            lock (m_TypeDefinitionsByName)
            {
                var types = from type in m_TypeDefinitionsByName[elementName]
                            where type.TypeId == elementType
                            select type;
                if (types.Count() == 1)
                {
                    return types.First();
                }
            }

            XElement typeNode = FindTypeNode(elementName, s_TypeIdsToXmlTags[elementType]);
            TypeDefinition typeDefinition = null;
            if (typeNode != null)
            {
                typeDefinition = TypeDefinition.Create(this, typeNode);
                if (typeDefinition.Ref.HasValue)
                {
                    lock (m_TypeDefinitionsByRefId)
                    {
                        m_TypeDefinitionsByRefId[typeDefinition.Ref.Value] = typeDefinition;
                    }
                }

                lock (m_TypeDefinitionsByName)
                {
                    m_TypeDefinitionsByName.Add(typeDefinition.Name, typeDefinition);
                }

                // Now that a placeholder for this type has been added to the two dictionaries it's safe to
                // complete the construction even if there's a recursive reference back to this item
                typeDefinition.FinishConstruction(typeNode, this);
            }

            return typeDefinition;
        }

        /// <summary>
        /// Finds the switch structure definition
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="xmlTag">The XML tag.</param>
        /// <returns>
        /// XML element defining the switch
        /// </returns>
        private XElement FindTypeNode(string elementName, string xmlTag)
        {
            // Select a node with the required tag that contains a name with the required value
            var nodes = from node in m_DataDictionaryXml.Descendants(xmlTag)
                        where node.Element("Name") != null && node.Element("Name").Value == elementName
                        select node;

            XElement typeNode = nodes.FirstOrDefault();

            return typeNode;
        }

        /// <summary>
        /// Return a type definition identified by its Ref ID
        /// </summary>
        /// <param name="typeRefId"></param>
        /// <returns>Type definition identified by its Ref ID</returns>
        internal TypeDefinition GetTypeDefinition(int typeRefId)
        {
            TypeDefinition theTypeDefinition = null;

            lock (m_TypeDefinitionsByRefId)
            {
                // See if the type has already been parsed. If not then parse it.
                if (!m_TypeDefinitionsByRefId.TryGetValue(typeRefId, out theTypeDefinition))
                {
                    // Look for a node whose Ref value matches the wanted ID
                    var refNode = m_DataDictionaryXml.Descendants("Ref").FirstOrDefault(x => x.Value == typeRefId.ToString());
                    if (refNode == null)
                    {
                        throw new DataDictionaryException(String.Format("Ref {0} not found", typeRefId));
                    }

                    var typeNode = refNode.Parent;

                    theTypeDefinition = TypeDefinition.Create(this, typeNode);
                    if (theTypeDefinition.Ref.HasValue)
                    {
                        m_TypeDefinitionsByRefId[theTypeDefinition.Ref.Value] = theTypeDefinition;
                    }

                    lock (m_TypeDefinitionsByName)
                    {
                        m_TypeDefinitionsByName.Add(theTypeDefinition.Name, theTypeDefinition);
                    }

                    // Now that a placeholder for this type has been added to the two dictionaries it's safe to
                    // complete the construction even if there's a recursive reference back to this item
                    theTypeDefinition.FinishConstruction(typeNode, this);
                }
            }

            return theTypeDefinition;
        }

        /// <summary>
        /// Return a nested type definition
        /// </summary>
        /// <param name="theOwningNode">The node.</param>
        /// <returns>
        /// Type definition identified by its Ref ID
        /// </returns>
        public TypeDefinition GetNestedTypeDefinition(XContainer theOwningNode)
        {
            TypeDefinition theTypeDefinition = null;

            HashSet<string> typeNames = new HashSet<string>(new string[] { "Switch", "ArrayType", "Struct", "Enum" });

            var elements = from element in theOwningNode.Elements()
                           where typeNames.Contains(element.Name.LocalName)
                           select element;

            if (elements.Count() > 0)
            {
                XElement theNode = elements.First();
                theTypeDefinition = TypeDefinition.Create(this, theNode);
                theTypeDefinition.FinishConstruction(theNode, this);
            }

            return theTypeDefinition;
        }

        /// <summary>
        /// Initializes static members of the <see cref="DictionaryManager"/> class.
        /// </summary>
        static DictionaryManager()
        {
            s_ObjectNamesToTypes["struct"] = TypeId.StructType;
            s_ObjectNamesToTypes["union"] = TypeId.UnionType;
            s_ObjectNamesToTypes["typedef"] = TypeId.TypeDefType;
            s_ObjectNamesToTypes["enum"] = TypeId.EnumType;
            s_ObjectNamesToTypes["basetype"] = TypeId.BaseType;
            s_ObjectNamesToTypes["switch"] = TypeId.SwitchType;

            s_TypeIdsToXmlTags[TypeId.StructType] = "Struct";
            s_TypeIdsToXmlTags[TypeId.UnionType] = "Union";
            s_TypeIdsToXmlTags[TypeId.TypeDefType] = "TypeDef";
            s_TypeIdsToXmlTags[TypeId.EnumType] = "Enum";
            s_TypeIdsToXmlTags[TypeId.BaseType] = "BaseType";
            s_TypeIdsToXmlTags[TypeId.SwitchType] = "Switch";

            StringDefinition stringDefinition = new StringDefinition ();
            s_BuiltInTypes[stringDefinition.Name] = stringDefinition;
        }

        /// <summary>
        /// Tree of XML nodes parsed from the xml text file
        /// </summary>
        private XElement m_DataDictionaryXml;

        /// <summary>
        /// Collection of signals indexed by name
        /// </summary>
        private Dictionary<string, SignalDescription> m_SignalDescriptionsByName = new Dictionary<string, SignalDescription>();

        /// <summary>
        /// Collection of signals indexed by id ('Value' in the DD XML)
        /// </summary>
        private Dictionary<uint, SignalDescription> m_SignalDescriptionsById = new Dictionary<uint, SignalDescription>();

        /// <summary>
        /// Collection of processes indexed by name
        /// </summary>
        private Dictionary<string, ProcessDescription> m_ProcessDescriptionsByName = new Dictionary<string, ProcessDescription>();

        /// <summary>
        /// Collection of processes indexed by ID
        /// </summary>
        private Dictionary<uint, ProcessDescription> m_ProcessDescriptionsById = new Dictionary<uint, ProcessDescription>();

        /// <summary>
        /// Collection of type definitions indexed by unique reference ID extracted from the DD
        /// </summary>
        private Dictionary<int, TypeDefinition> m_TypeDefinitionsByRefId = new Dictionary<int, TypeDefinition>();

        /// <summary>
        /// The type definitions by name
        /// </summary>
        private MultiDictionary<string, TypeDefinition> m_TypeDefinitionsByName;

        /// <summary>
        /// The XMLObject type names (from the TSE data dict spec) mapped to corresponding types 
        /// </summary>
        private static Dictionary<string, TypeId> s_ObjectNamesToTypes = new Dictionary<string, TypeId>();

        /// <summary>
        /// Maps type ids to XML tag strings
        /// </summary>
        private static Dictionary<TypeId, string> s_TypeIdsToXmlTags = new Dictionary<TypeId, string>();

        /// <summary>
        /// Maps names of built-in types to their corresponding types 
        /// </summary>
        private static Dictionary<string, TypeDefinition> s_BuiltInTypes = new Dictionary<string, TypeDefinition>();

        /// <summary>
        /// Collection of signal description objects
        /// </summary>
        private ReadOnlyCollection<SignalDescription> m_Signals;

        /// <summary>
        /// Collection of process description objects
        /// </summary>
        private ReadOnlyCollection<ProcessDescription> m_Processes;

        /// <summary>
        /// The unique id
        /// </summary>
        private int m_UniqueId;

        /// <summary>
        /// The trace source
        /// </summary>
        private TraceSource m_TraceSource = DataDictionaryTraceSource.TraceSource;

        /// <summary>
        /// List of types that have been instantiated but not yet fully constructed
        /// </summary>
        private List<TypeDefinition> m_TypesPendingConstruction = new List<TypeDefinition>();
    }
}
