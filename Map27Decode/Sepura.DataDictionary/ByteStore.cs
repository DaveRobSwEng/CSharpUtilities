#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// ByteStore.cs
//  Implementation of the Class ByteStore
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
    using System.IO;
    using System.Linq;
    
    /// <summary>
    /// Class containing a sequence of bytes and an index into the current position
    /// within the sequence.
    /// </summary>
    public class ByteStore 
    {
        /// <summary>
        /// Gets the index of the current read position.
        /// </summary>
        public int ReadPosition { get; private set; }

        /// <summary>
        /// Gets the index of the current write position.
        /// </summary>
        public int WritePosition 
        { 
            get
            {
                return m_Bytes.Count;
            }
        }

        /// <summary>
        /// Gets the payload as an enumerable set of bytes
        /// </summary>
        public IEnumerable<byte> Payload
        {
            get
            {
                return m_Bytes;
            }
        }

        /// <summary>
        /// Gets the length of the payload.
        /// </summary>
        /// <value>
        /// The length of the payload.
        /// </value>
        public int PayloadLength
        {
            get { return m_Bytes.Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteStore"/> class.
        /// </summary>
        public ByteStore()
        {
            m_Bytes = new List<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteStore"/> class populated with a collection
        /// of bytes
        /// </summary>
        /// <param name="theBytes">The bytes to store in the internal store</param>
        public ByteStore(IEnumerable<byte> theBytes)
        {
            m_Bytes = new List<byte>(theBytes);
        }

        /// <summary>
        /// Reads a single byte from the sequence at the current read location and
        /// increments the current read location
        /// </summary>
        /// <returns>The extracted value</returns>
        public byte GetByte()
        {
            if (ReadPosition >= m_Bytes.Count)
            {
                throw new DataDictionaryException("Read beyond end of data");
            }

            return m_Bytes[ReadPosition++];
        }

        /// <summary>
        /// Reads a 16-bit value from the buffer
        /// </summary>
        /// <returns>Value read from the buffer</returns>
        public ushort GetUint16()
        {
            ushort result = 0;

            if (DictionaryManager.Endianism == Endianism.BigEndian)
            {
                result = GetByte();
                result = (ushort)((result << 8) + GetByte());
            }
            else
            {
                result = (ushort) GetByte();
                result += (ushort)(GetByte() << 8);
            }

            return result;
        }

        /// <summary>
        /// Reads a 32-bit value from the buffer
        /// </summary>
        /// <returns>Value read from the buffer</returns>
        public uint GetUint32()
        {
            uint result = 0;
            if (DictionaryManager.Endianism == Endianism.BigEndian)
            {
                result = GetByte();
                result = (uint)((result << 8) + GetByte());
                result = (uint)((result << 8) + GetByte());
                result = (uint)((result << 8) + GetByte());
            }
            else
            {
                result = (uint)GetByte();
                result += (uint)(GetByte() << 8);
                result += (uint)(GetByte() << 16);
                result += (uint)(GetByte() << 24);
            }

            return result;
        }

        /// <summary>
        /// Reads a 64-bit value from the buffer
        /// </summary>
        /// <returns>Value read from the buffer</returns>
        public ulong GetUint64()
        {
            ulong result = 0;
            if (DictionaryManager.Endianism == Endianism.BigEndian)
            {
                for (int i = 0; i < 8; i++)
                {
                    result = (result << 8) + GetByte();
                }
            }
            else
            {
                int shiftBits = 0;
                for (int i = 0; i < 8; i++)
                {
                    result += (ulong)GetByte() << shiftBits;
                    shiftBits += 8;
                }
            }

            return result;
        }

        /// <summary>
        /// Adds a single byte to the sequence at the end of the sequence
        /// </summary>
        /// <param name="theByte">The byte.</param>
        public void PutByte(byte theByte)
        {
            m_Bytes.Add(theByte);
        }

        /// <summary>
        /// Puts the 16-bit value into the store
        /// </summary>
        /// <param name="theValue">The value.</param>
        public void PutUint16 (ushort theValue)
        {
            if (DictionaryManager.Endianism == Endianism.BigEndian)
            {
                // Big endian - write the MSByte first
                ushort shiftBits = 8;
                ushort mask = (ushort)(0xff << shiftBits);
                while (shiftBits > 0)
                {
                    m_Bytes.Add((byte)((theValue & mask) >> shiftBits));
                    mask >>= 8;
                    shiftBits -= 8;
                }

                m_Bytes.Add((byte)(theValue & mask));
            }
            else
            {
                // Little endian - write the LSByte first
                ushort mask = 0xff;
                ushort shiftBits = 0;
                do
                {
                    m_Bytes.Add((byte)(theValue & mask));
                    theValue >>= 8;
                    shiftBits += 8;
                } 
                while (shiftBits <= 8);
            }
        }

        /// <summary>
        /// Puts the 32-bit value into the store
        /// </summary>
        /// <param name="theValue">The value.</param>
        public void PutUint32(uint theValue)
        {
            if (DictionaryManager.Endianism == Endianism.BigEndian)
            {
                // Big endian - write the MSByte first
                int shiftBits = 24;
                uint mask = (uint)((uint)0xff << shiftBits);
                while (shiftBits > 0)
                {
                    m_Bytes.Add((byte)((theValue & mask) >> shiftBits));
                    mask >>= 8;
                    shiftBits -= 8;
                }

                m_Bytes.Add((byte)(theValue & mask));
            }
            else
            {
                // Little endian - write the LSByte first
                uint mask = 0xff;
                uint shiftBits = 0;
                do
                {
                    m_Bytes.Add((byte)(theValue & mask));
                    theValue >>= 8;
                    shiftBits += 8;
                } 
                while (shiftBits <= 24);
            }
        }

        /// <summary>
        /// Puts the 64-bit value into the store
        /// </summary>
        /// <param name="theValue">The value.</param>
        public void PutUint64(ulong theValue)
        {
            if (DictionaryManager.Endianism == Endianism.BigEndian)
            {
                // Big endian - write the MSByte first
                int shiftBits = 56;
                ulong mask = (ulong)(0xff << shiftBits);
                while (shiftBits > 0)
                {
                    m_Bytes.Add((byte)((theValue & mask) >> shiftBits));
                    mask >>= 8;
                    shiftBits -= 8;
                }

                m_Bytes.Add((byte)(theValue & mask));
            }
            else
            {
                // Little endian - write the LSByte first
                ulong mask = 0xff;
                uint shiftBits = 0;
                do
                {
                    m_Bytes.Add((byte)(theValue & mask));
                    theValue >>= 8;
                    shiftBits += 8;
                } 
                while (shiftBits <= 56);
            }
        }

        /// <summary>
        /// Removes the contents of the payload and resets the read/write locations
        /// </summary>
        public void Clear()
        {
            m_Bytes.Clear();
            ReadPosition = 0;
        }

        /// <summary>
        /// Sequence of bytes managed by this object
        /// </summary>
        private readonly List<byte> m_Bytes;
    }
}