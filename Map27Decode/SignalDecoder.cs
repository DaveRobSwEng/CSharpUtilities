using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sepura.DataDictionary;

namespace Map27Decode
{
    internal class SignalDecoder
    {
        internal SignalDecoder(FileInfo dataDictionary)
        {
            m_DictionaryManager.Load(dataDictionary);
        }

        /// <summary>
        /// Gets the expanded text for the signal by decoding the signal content using the data dictionary
        /// </summary>
        /// <returns>
        /// Expanded text for the signal: decoded signal content
        /// </returns>
        internal string GetSignalText(uint theSignalMessageId, byte[] theMessageData)
        {
            if (m_DictionaryManager == null || !m_DictionaryManager.IsLoaded)
            {
                return "Cannot decode the signal: No data dictionary loaded";
            }

            StringBuilder signalText = new StringBuilder();
            try
            {
                SignalDescription signalDescription = m_DictionaryManager.GetSignalDescription(theSignalMessageId);
                TypeDefinition signalType = m_DictionaryManager.GetSignalTypeDefinition(theSignalMessageId);

                signalText.AppendFormat("{0} (0x{1:x2})",
                    signalDescription == null ? "Unknown signal ID" : signalDescription.Name,
                    signalDescription == null ? theSignalMessageId : signalDescription.Value);

                if (signalType != null && signalDescription != null)
                {
                    signalText.Append("\n");
                    Value signalElement = GetSignalValue(theSignalMessageId, theMessageData);
                    if (signalElement != null)
                    {
                        EncodeSignalElement(signalElement, signalText, string.Empty, signalDescription.Name);
                    }
                    else
                    {
                        signalText.Append("Error decoding signal");
                    }
                }
            }
            catch (DataDictionaryException ex)
            {
                signalText = new StringBuilder(string.Format("Error decoding signal: {0}", ex.Message));
            }

            return signalText.ToString();
        }

        /// <summary>
        /// Encodes the signal element in
        /// </summary>
        /// <param name="signalElement">The signal element.</param>
        /// <param name="signalText">The signal text.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="valueName">Name of the value.</param>
        private void EncodeSignalElement(Value signalElement, StringBuilder signalText, string indent, string valueName)
        {
            switch (signalElement.FundamentalType.TypeId)
            {
                case TypeId.StructType:
                    EncodeStructType(signalElement as StructureValue, signalText, indent + "  ", valueName);
                    break;
                case TypeId.BaseType:
                    EncodeBaseType(signalElement as BaseTypeValue, signalText, indent + "  ", valueName);
                    break;
                case TypeId.EnumType:
                    EncodeEnumType(signalElement as EnumValue, signalText, indent + "  ", valueName);
                    break;
                case TypeId.ArrayType:
                    EncodeArrayType(signalElement as ArrayTypeValue, signalText, indent + "  ", valueName);
                    break;
                default:
                    signalText.AppendFormat("{0}Cannot display {1} type {2}", indent, valueName, signalElement.FundamentalType.Name);
                    break;
            }
        }

        /// <summary>
        /// Encodes the type of the array.
        /// </summary>
        /// <param name="arrayTypeValue">The array type value.</param>
        /// <param name="signalText">The signal text.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="valueName">Name of the value.</param>
        private void EncodeArrayType(ArrayTypeValue arrayTypeValue, StringBuilder signalText, string indent, string valueName)
        {
            signalText.AppendFormat("{0}{1}:\n", indent, valueName);

            ArrayTypeDefinition arrayDefinition = arrayTypeValue.FundamentalType as ArrayTypeDefinition;

            if (arrayDefinition.Rank > 0)
            {
                DisplayFixedSizeArrayValueMembers(signalText, indent, valueName, arrayTypeValue, arrayDefinition);
            }
            else
            {
                DisplayVariableSizeArrayValueMembers(signalText, indent, valueName, arrayTypeValue, arrayDefinition);
            }
        }

        /// <summary>
        /// Displays the variable size array value members.
        /// </summary>
        /// <param name="signalText">The signal text.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="arrayTypeValue">The array type value.</param>
        /// <param name="arrayDefinition">The array definition.</param>
        private void DisplayVariableSizeArrayValueMembers(StringBuilder signalText, string prefix, string valueName, ArrayTypeValue arrayTypeValue, ArrayTypeDefinition arrayDefinition)
        {
            // Iterate over the values (remembering Lua indices are 1-based
            int index = 1;
            foreach (var item in arrayTypeValue.Values)
            {
                StringBuilder itemIndexText = new StringBuilder(valueName);
                itemIndexText.AppendFormat("[{0}]", index);
                index += 1;

                EncodeSignalElement(item, signalText, prefix, itemIndexText.ToString());
            }
        }

        /// <summary>
        /// Displays the fixed size array value members.
        /// </summary>
        /// <param name="signalText">The signal text.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="arrayTypeValue">The array type value.</param>
        /// <param name="arrayDefinition">The array definition.</param>
        private void DisplayFixedSizeArrayValueMembers(StringBuilder signalText, string indent, string valueName, ArrayTypeValue arrayTypeValue, ArrayTypeDefinition arrayDefinition)
        {
            // Set up a (N-1)-dimensional array to contain sub-arrays. The final Nth dimension holds
            // simple values rather than arrays.
            int[] indices = new int[arrayDefinition.Rank - 1];
            for (int i = 0; i < arrayDefinition.Rank - 1; i++)
            {
                indices[i] = arrayDefinition.UpperBound[i];
            }

            // Iterate over the indices array generating script to create the N-d array
            DepthFirstArrayIterator arrayCreationIterator = new DepthFirstArrayIterator(indices);
            foreach (var item in arrayCreationIterator)
            {
                StringBuilder itemIndexText = new StringBuilder(valueName);
                for (int j = 0; j < item.Length; j++)
                {
                    // Add 1 to index because Lua uses 1-based indices
                    itemIndexText.AppendFormat("[{0}]", item[j] + 1);
                }

                signalText.AppendFormat("{0}{1} = {{}}      -- create empty table/array\n", indent, itemIndexText);
            }

            // Iterate over the values
            arrayTypeValue.Iterate(
                (arrayValue, arrayIndices) =>
                {
                    StringBuilder itemIndexText = new StringBuilder(valueName);
                    for (int i = 0; i < arrayIndices.Length; i++)
                    {
                        // Add 1 to index because Lua uses 1-based indices
                        itemIndexText.AppendFormat("[{0}]", arrayIndices[i] + 1);
                    }

                    EncodeSignalElement(arrayValue, signalText, indent, itemIndexText.ToString());
                });
        }

        /// <summary>
        /// Encodes the type of the enum.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="signalText">The signal text.</param>
        /// <param name="indent">The indent.</param>
        /// <param name="valueName">Name of the value.</param>
        private void EncodeEnumType(EnumValue enumValue, StringBuilder signalText, string indent, string valueName)
        {
            signalText.AppendFormat("{0}{1} {2} = [{3}] {4}\n", indent, enumValue.InitialType.Name, valueName, enumValue.IntegerValue, enumValue.StringValue);
        }

        /// <summary>
        /// Encodes the type of the base.
        /// </summary>
        /// <param name="baseTypeValue">The base type value.</param>
        /// <param name="signalText">The signal text.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="valueName">Name of the value.</param>
        private void EncodeBaseType(BaseTypeValue baseTypeValue, StringBuilder signalText, string prefix, string valueName)
        {
            // Don't generate script for 0-size items because these are special-case markers
            if (baseTypeValue.GetSizeBytes() > 0)
            {
                if (baseTypeValue.IsSigned)
                {
                    signalText.AppendFormat("{0}{1} {2} = {3} (0x{3:x})\n", prefix, baseTypeValue.InitialType.Name, valueName, baseTypeValue.SignedValue);
                }
                else
                {
                    signalText.AppendFormat("{0}{1} {2} = {3} (0x{3:x})\n", prefix, baseTypeValue.InitialType.Name, valueName, baseTypeValue.UnsignedValue);
                }
            }
        }

        /// <summary>
        /// Encodes the type of the struct.
        /// </summary>
        /// <param name="structureValue">The structure value.</param>
        /// <param name="signalText">The signal text.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="valueName">Name of the value.</param>
        private void EncodeStructType(StructureValue structureValue, StringBuilder signalText, string prefix, string valueName)
        {
            signalText.AppendFormat("{0}{1} {2}:\n", prefix, structureValue.InitialType.Name, valueName);
            foreach (var attribute in structureValue.Attributes)
            {
                EncodeSignalElement(attribute.Value, signalText, prefix, attribute.Name);
            }
        }

        /// <summary>
        /// Gets the signal value.
        /// </summary>
        /// <returns>Decoded signal value or null if the decode fails</returns>
        public Value GetSignalValue(uint theSignalMessageId, byte[] theMessageData)
        {
            if (m_DictionaryManager == null || !m_DictionaryManager.IsLoaded)
            {
                return null;
            }

            Value signalValue = null;
            TypeDefinition signalType = m_DictionaryManager.GetSignalTypeDefinition(theSignalMessageId);
            if (signalType != null)
            {
                signalValue = signalType.Decode(new ByteStore(theMessageData), null);
            }

            return signalValue;
        }

        readonly DictionaryManager m_DictionaryManager = new DictionaryManager();
    }
}
