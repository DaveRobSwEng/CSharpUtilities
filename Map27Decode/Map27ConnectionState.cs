using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sepura.DataDictionary;

namespace Map27Decode
{
    internal class Map27ConnectionState
    {
        public ulong Ns
        {
            get;
            private set;
        }

        public ulong Nr
        {
            get;
            private set;
        }

        public Map27LinkState Map27LinkState
        {
            get;
            private set;
        }

        public Map27ConnectionState()
        {
            Map27LinkState = Map27LinkState.ResetWait;
        }

        internal string[] HandleFrame(StructureValue map27SignalStruct, double timestamp, string direction)
        {
            AttributeValue map27FrameAttribute = (AttributeValue)map27SignalStruct.Attributes[0];
            StructureValue map27FrameStruct = (StructureValue)map27FrameAttribute.Value;

            string[] analysisReports = null;

            switch (map27FrameAttribute.Name)
            {
                case "LR":
                    Map27LinkState = Map27LinkState.LinkWait;
                    Nr = 1;
                    Ns = 0;
                    break;
                case "LA":
                    Map27LinkState = Map27LinkState.Ready;
                    break;
                case "LT":
                    analysisReports = HandleLtFrame(map27FrameStruct, timestamp, direction);
                    break;
            }

            return analysisReports;
        }

        private string[] HandleLtFrame(StructureValue map27FrameStruct, double timestamp, string direction)
        {
            List<string> analysisReports = null;

            AttributeValue nsAttribute = (AttributeValue)map27FrameStruct.Attributes[1];

            ulong rxFrameNs = nsAttribute.Value.GetUlongValue();

            if (rxFrameNs == Nr)
            {
                Nr = Nr.PlusOneMod256();
            }
            else
            {
                if (rxFrameNs == Nr.LessOneMod256())
                {
                    // Retransmission of previous frames
                    analysisReports = AddStringToList(analysisReports, string.Format("{0} Retransmission N(S) = {1} ({2:F4}s)", direction, rxFrameNs, (timestamp - m_PreviousLtFrameTimestamp)));
                }
                else
                {
                    // Out of sequence frame
                    analysisReports = AddStringToList(analysisReports, string.Format("{0} Out of sequence frame: Current N(R) = {1}, received N(S) = {2}", direction, Nr, rxFrameNs));
                }
            }

            m_PreviousLtFrameTimestamp = timestamp;

            return analysisReports == null ? null : analysisReports.ToArray();
        }

        private static List<string> AddStringToList(List<string> analysisReports, string report)
        {
            if (analysisReports == null)
            {
                analysisReports = new List<string>();
            }

            analysisReports.Add(report);

            return analysisReports;
        }

        private double m_PreviousLtFrameTimestamp;
    }
}
