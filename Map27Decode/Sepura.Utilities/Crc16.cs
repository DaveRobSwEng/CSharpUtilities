#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// Crc16.cs
//  Implementation of the Class Crc16
//
// Copyright (c) 2010 Sepura Plc
// All Rights reserved.
//
//  Original author: RobinsonD
//
// $Id:$
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class to calculate CRC16 over byte arrays
    /// Generator polynomial G(x) = x16 + x15 + x2 + 1. 
    /// Code taken from MAP27 API appendix A1
    /// </summary>
    public static class Crc16
    {
        /// <summary>
        /// Initializes static members of the Crc16 class.
        /// Calculate the static modification table
        /// </summary>
        static Crc16()
        {
            CreateTable();
        }

        /// <summary>
        /// Calculates the FCS sequence over a buffer
        /// Note: fcs is initialised with all ones
        /// </summary>
        /// <param name="input">Buffer to process</param>
        /// <param name="startOffset">Offset into the buffer at which to start processing</param>
        /// <returns>Calculated CRC</returns>
        public static int CalculateCrc(byte[] input, int startOffset)
        {
            return CalculateCrc(input, startOffset, (ushort)(input.Length - startOffset));
        }

        /// <summary>
        /// Calculates the CRC over a sequential list of buffers
        /// Note: CRC is initialised with all ones
        /// </summary>
        /// <param name="buffers">List of buffers to process</param>
        /// <returns>Calculated CRC</returns>
        public static int CalculateCrc(List<List<byte>> buffers)
        {
            ushort fcs;     // frame check sequence 
            ushort q;       // calculation register 
            fcs = 0xffff;   // fcs initialised with all ones 

            foreach (var buffer in buffers)
            {
                foreach (var theOctet in buffer)
                {
                    q = mtab[(theOctet ^ (fcs >> 8))];
                    fcs = (ushort)(((q & 0xff00) ^ (fcs << 8)) | (q & 0x00ff));
                }
            }

            return (ushort)(fcs ^ 0xffff);  // return the fcs ones complement
        }

        /// <summary>
        /// Produce the look-up table
        /// </summary>
        private static void CreateTable()
        {
            ushort[] btab = new ushort[8];  // table btab
            ushort i, j;    // loop parameters
            ushort q;   // calculation register
            ushort shreg;   // shift-register
            ushort carry, bit;  // bit parameters

            // Calculate the table btab:
            carry = 1;  // carry flag set to one
            shreg = 0;  // shreg initialised with 0
            for (i = 0; i < 8; i++)
            {
                if (0 != carry)
                {
                    shreg ^= CRC16;
                }

                btab[i] = (ushort)((shreg << 8) | (shreg >> 8));    // swap bytes
                carry = (ushort)(shreg & 1);
                shreg >>= 1;
            }

            // Calculate the modification table mtab:
            int mtabIndex = 0;
            for (i = 0; i < 256; i++)
            {
                q = 0;
                bit = 0x80;
                for (j = 0; j < 8; j++)
                {
                    if (0 != (bit & i))
                    {
                        q ^= btab[j];
                    }

                    bit >>= 1;
                }

                mtab[mtabIndex++] = q;
            }
        }

        /// <summary>
        /// Calculates the CRC
        /// Note: CRC is initialised with all ones
        /// </summary>
        /// <param name="buff">Pointer to character buffer</param>
        /// <param name="startOffset">Offset into buffer of first byte to process</param>
        /// <param name="len">Number of bytes to process</param>
        /// <returns>FCS frame check sequence</returns>
        private static int CalculateCrc(byte[] buff, int startOffset, ushort len)
        {
            ushort fcs;     // frame check sequence
            ushort q;       // calculation register
            int bufIndex = startOffset;

            fcs = 0xffff;   // fcs initialised with all ones
            while (0 != len--)
            {
                q = mtab[(buff[bufIndex++] ^ (fcs >> 8))];
                fcs = (ushort)(((q & 0xff00) ^ (fcs << 8)) | (q & 0x00ff));
            }

            return fcs ^ 0xffff;  // return the fcs ones complement
        }

        /// <summary>
        /// CRC 16 constant representing the polynomial (reversed)
        /// </summary>
        private const ushort CRC16 = 0xA001;         

        /// <summary>
        /// Modification table used to calculate successive CRC values
        /// </summary>
        private static ushort[] mtab = new ushort[256]; 
    }
}
