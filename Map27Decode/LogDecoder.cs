using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Sepura.DataDictionary;
using Sepura.Utilities;

namespace Map27Decode
{
    internal class LogDecoder
    {
        public delegate void HandleDecodedSignal(string signalText);

        public event HandleDecodedSignal OnSignalDecoded;

        public event HandleDecodedSignal OnOutOfFrameBytes;

        public bool ShowRetries
        {
            get;
            set;
        }

        internal void Decode(string fileName, string dataDictionary)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception(string.Format("Log file {0} does not exist", fileName));
            }

            if (!File.Exists(dataDictionary))
            {
                throw new Exception(string.Format("Data dictionary file {0} does not exist", dataDictionary));
            }

            DictionaryManager.Endianism = Endianism.BigEndian;

            m_SignalDecoder = new SignalDecoder(new FileInfo(dataDictionary));

            IMap27PacketDecoder decoder = null;
            if (EllisysRawPacketDecoder.CanDecodeLogFile(fileName))
            {
                decoder = new EllisysRawPacketDecoder();
            }
            else if (Map27ApiLogDecoder.CanDecodeLogFile (fileName))
            {
                decoder = new Map27ApiLogDecoder();
            }
            else if (UsbPcapDecoder.CanDecodeLogFile(fileName))
            {
                decoder = new UsbPcapDecoder();
            }
            else
            {
                throw new Exception(string.Format("Cannot recognise format of input file {0}", fileName));
            }

            decoder.OnRxData += Decoder_OnRxData;
            decoder.OnTxData += Decoder_OnTxData;

            m_RxMapDecoder.OnProcessingComplete += RxMapDecoder_OnProcessingComplete;
            m_TxMapDecoder.OnProcessingComplete += TxMapDecoder_OnProcessingComplete;

            decoder.ReadFile(new FileInfo(fileName));
        }

        private void RxMapDecoder_OnProcessingComplete(TapStreamDecoder.ProcessingResult result)
        {
            OnProcessingComplete(result, m_RxMapDecoder, "Rx  ", m_RxState);
        }

        private void TxMapDecoder_OnProcessingComplete(TapStreamDecoder.ProcessingResult result)
        {
            OnProcessingComplete(result, m_TxMapDecoder, "  Tx", m_TxState);
        }

        private void OnProcessingComplete(TapStreamDecoder.ProcessingResult result, TapStreamDecoder decoder, string direction, Map27ConnectionState theConnectionState)
        {
            switch (result)
            {
                case TapStreamDecoder.ProcessingResult.Ok:
                    ReportDecodedSignal(decoder.Map27Packet.ToArray(), decoder.MessageEndTimeText, decoder.MessageEndTime, direction, theConnectionState);
                    break;
                case TapStreamDecoder.ProcessingResult.FcsError:
                    ReportError("FCS error", decoder.MessageEndTimeText, decoder.MessageEndTime, direction);
                    break;
                case TapStreamDecoder.ProcessingResult.FormatError:
                    ReportError("Format error", decoder.MessageEndTimeText, decoder.MessageEndTime, direction);
                    break;
                case TapStreamDecoder.ProcessingResult.OutOfFrameBytesFound:
                    ReportOutOfFrameBytes(decoder.OutOfFrameBytes, decoder.MessageEndTimeText, decoder.MessageEndTime, direction);
                    break;
                default:
                    ReportError("Unexpected processing result", decoder.MessageEndTimeText, decoder.MessageEndTime, direction);
                    break;
            }
        }

        private void ReportOutOfFrameBytes(ReadOnlyCollection<byte> outOfFrameBytes, string messageEndTimeText, double messageEndTime, string direction)
        {
            if (OnOutOfFrameBytes != null)
            {
                // See if there's any ASCII text
                StringBuilder sb = new StringBuilder();
                Decoder theDecoder = Encoding.ASCII.GetDecoder();
                foreach (byte theByte in outOfFrameBytes)
                {
                    char decoded = (char)theByte;
                    switch (decoded)
                    {
                        case '\r':
                            sb.Append(@"\r");
                            break;
                        case '\n':
                            sb.Append(@"\n");
                            break;
                        default:
                            if (Char.IsSymbol(decoded) || Char.IsLetterOrDigit(decoded) || Char.IsPunctuation(decoded))
                            {
                                sb.Append(decoded);
                            }
                            else
                            {
                                sb.Append(".");
                            }
                            break;
                    }
                }

                OnOutOfFrameBytes(string.Format("{0} {1:0000.000000} {2} [Out of frame] {3} {4}", 
                    messageEndTimeText, 
                    messageEndTime, 
                    direction, 
                    Formatting.BinToHex(outOfFrameBytes, ' '),
                    sb.ToString()));
            }
        }

        private void ReportAsciiSignal(string asciiString, string messageEndTimeText, double messageEndTime, string direction)
        {
            if (OnSignalDecoded != null)
            {
                OnSignalDecoded(string.Format("{0} {1:0000.000000} {2}  {3}", messageEndTimeText, messageEndTime, direction, asciiString));
            }
        }

        private void ReportError(string errorString, string messageEndTimeText, double messageEndTime, string direction)
        {
            if (OnSignalDecoded != null)
            {
                OnSignalDecoded(string.Format("{0} {1:0000.000000} {2}  {3}", messageEndTimeText, messageEndTime, direction, errorString));
            }
        }

        private void ReportDecodedSignal(byte[] map27Packet, string logTime, double timestamp, string direction, Map27ConnectionState theConnectionState)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1:0000.000000} {2}  ", logTime, timestamp, direction);

            try
            {
                Value map27Signal = null;
                try
                {
                    map27Signal = m_SignalDecoder.GetSignalValue(map27Packet[0], map27Packet);
                }
                catch (PartialDecodeException partialDecodeException)
                {
                    map27Signal = partialDecodeException.ValueBeingDecoded;
                }

                StructureValue map27SignalStruct = map27Signal as StructureValue;
                if (map27SignalStruct != null && map27SignalStruct.Attributes.Count> 0)
                {
                    AttributeValue map27FrameAttribute = map27SignalStruct.Attributes[0];
                    StructureValue map27FrameStruct = map27FrameAttribute.Value as StructureValue;

                    if(map27FrameStruct != null)
                    {
                        string[] analysisResults = null;
                        if (ShowRetries)
                        {
                            analysisResults = theConnectionState.HandleFrame(map27SignalStruct, timestamp, direction);
                        }

                        // Report the elements of the MAP27 frame
                        foreach (var attribute in map27FrameStruct.Attributes)
                        {
                            EnumValue theEnumValue = attribute.Value as EnumValue;
                            if (theEnumValue != null)
                            {
                                sb.AppendFormat("{0}:{1} ", attribute.Name, theEnumValue.StringValue, theEnumValue.IntegerValue);
                            }
                            else
                            {
                                sb.AppendFormat("{0}:{1} ", attribute.Name, attribute.Value);
                            }
                        }

                        // Append the raw data to the end of the MAP27 frame
                        sb.AppendFormat("    {0}", Formatting.BinToHex(map27Packet, ' '));

                        // Report the contents of the TAP frame
                        if (map27SignalStruct.Attributes.Count > 1)
                        {
                            AttributeValue tapFrameAttribute = map27SignalStruct.Attributes[1];
                            StructureValue tapFrameStruct = tapFrameAttribute.Value as StructureValue;

                            if (tapFrameStruct != null)
                            {
                                // ShowStructOnSingleLine(sb, tapFrameStruct);
                                ShowTapFrameOnMultipleLines(sb, tapFrameStruct, sb.ToString().IndexOf("Type", StringComparison.Ordinal));
                            }
                            else
                            {
                                sb.AppendFormat("\n{0}", tapFrameAttribute.Value.ToString());
                            }
                        }

                        if (analysisResults != null)
                        {
                            sb.AppendFormat("\n[Analysis] {0}", string.Join("\n", analysisResults));
                        }
                    }
                }
            }
            catch (DataDictionaryException ex)
            {
                sb.AppendFormat("{0} [{1}]", Formatting.BinToHex(map27Packet, ' '), ex.Message);
            }

            if (OnSignalDecoded != null)
            {
                OnSignalDecoded(sb.ToString());
            }
        }

        private void ShowTapFrameOnMultipleLines(StringBuilder sb, StructureValue tapFrameStruct, int indentCount)
        {
            string indentString = string.Format("\n{0}", new string(' ', indentCount));
            sb.AppendFormat("{0}{1} Block:{2} Count:{3} Command:{4}",
                indentString,
                "TAP",  // GetIntegerValue(tapFrameStruct.Attributes["Type"].Value),
                GetIntegerValue(tapFrameStruct.Attributes["Block"].Value),
                GetIntegerValue(tapFrameStruct.Attributes["Count"].Value),
                GetEnumValue(tapFrameStruct.Attributes["Command"].Value));

            if (tapFrameStruct.Attributes.Count == 5 && tapFrameStruct.Attributes[4].Value is StructureValue)
            {
                //sb.AppendFormat("{0}{{", indentString);
                ShowStructOnMultipleLines(sb, (StructureValue)tapFrameStruct.Attributes[4].Value, indentCount + 2);
                //sb.AppendFormat("{0}}} ", indentString);
            }
        }

        private static string GetIntegerValue(Value theValue)
        {
            string integerValue = string.Empty;
            BaseTypeValue theBaseTypeValue = theValue as BaseTypeValue;
            if (theBaseTypeValue != null)
            {
                integerValue = theBaseTypeValue.IsSigned ? theBaseTypeValue.SignedValue.ToString() : theBaseTypeValue.UnsignedValue.ToString();
            }
            else
            {
                throw new InvalidDataException(string.Format("Unsupported type {0}", theValue.GetType()));
            }

            return integerValue;
        }

        private static string GetEnumValue(Value theValue)
        {
            string enumValue = string.Empty;
            EnumValue theEnumValue = theValue as EnumValue;
            if (theEnumValue != null)
            {
                enumValue = string.Format("{0} ({1})", theEnumValue.StringValue, theEnumValue.IntegerValue);
            }
            else
            {
                throw new InvalidDataException(string.Format("Unsupported type {0}", theValue.GetType()));
            }

            return enumValue;
        }

        private void ShowStructOnMultipleLines(StringBuilder sb, StructureValue theStruct, int indentCount)
        {
            string indentString = string.Format("\n{0}", new string(' ', indentCount));

            foreach (var attribute in theStruct.Attributes)
            {
                StructureValue theStructureValue = attribute.Value as StructureValue;
                EnumValue theEnumValue = null;
                if (theStructureValue != null)
                {
                    //sb.AppendFormat("{0}{{", indentString);
                    ShowStructOnMultipleLines(sb, theStructureValue, indentCount + 2);
                    //sb.AppendFormat("{0}}} ", indentString);
                }
                else if ((theEnumValue = attribute.Value as EnumValue) != null)
                {
                    sb.AppendFormat("{0}{1}:{2} ({3}) ", indentString, attribute.Name, theEnumValue.StringValue, theEnumValue.IntegerValue);
                }
                else
                {
                    sb.AppendFormat("{0}{1}:{2} ", indentString, attribute.Name, attribute.Value);
                }
            }
        }

        private static void ShowStructOnSingleLine(StringBuilder sb, StructureValue theStruct)
        {
            foreach (var attribute in theStruct.Attributes)
            {
                StructureValue theStructureValue = attribute.Value as StructureValue;
                EnumValue theEnumValue = null;
                if (theStructureValue != null)
                {
                    sb.Append("{");
                    ShowStructOnSingleLine(sb, theStructureValue);
                    sb.Append("} ");
                }
                else if ((theEnumValue = attribute.Value as EnumValue) != null)
                {
                    sb.AppendFormat("{0}:{1} ", attribute.Name, theEnumValue.StringValue, theEnumValue.IntegerValue);
                }
                else
                {
                    sb.AppendFormat("{0}:{1} ", attribute.Name, attribute.Value);
                }
            }
        }

        private void Decoder_OnRxData(byte[] data, int startPos, int length, string logTime, double timestamp)
        {
            m_RxMapDecoder.Decode(data, startPos, length, logTime, timestamp);
        }

        private void Decoder_OnTxData(byte[] data, int startPos, int length, string logTime, double timestamp)
        {
            m_TxMapDecoder.Decode(data, startPos, length, logTime, timestamp);
        }

        SignalDecoder m_SignalDecoder;

        TapStreamDecoder m_RxMapDecoder = new TapStreamDecoder();

        TapStreamDecoder m_TxMapDecoder = new TapStreamDecoder();

        Map27ConnectionState m_TxState = new Map27ConnectionState();

        Map27ConnectionState m_RxState = new Map27ConnectionState();
    }
}
