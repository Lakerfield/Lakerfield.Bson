﻿/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using Lakerfield.Shared;

namespace Lakerfield.Bson
{
    /// <summary>
    /// Represents BSON binary data.
    /// </summary>
    [Serializable]
    public class BsonBinaryData : BsonValue, IComparable<BsonBinaryData>, IEquatable<BsonBinaryData>
    {
        // private fields
        private readonly byte[] _bytes;
        private readonly BsonBinarySubType _subType;
        private readonly GuidRepresentation _guidRepresentation; // only relevant if subType is UuidStandard or UuidLegacy

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryData class.
        /// </summary>
        /// <param name="bytes">The binary data.</param>
        public BsonBinaryData(byte[] bytes)
            : this(bytes, BsonBinarySubType.Binary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonBinaryData class.
        /// </summary>
        /// <param name="bytes">The binary data.</param>
        /// <param name="subType">The binary data subtype.</param>
        public BsonBinaryData(byte[] bytes, BsonBinarySubType subType)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy)
            {
                if (bytes.Length != 16)
                {
                    var message = string.Format(
                        "Length must be 16, not {0}, when subType is {1}.",
                        bytes.Length, subType);
                    throw new ArgumentException(message, nameof(bytes));
                }
            }

            _bytes = bytes;
            _subType = subType;
        }

        /// <summary>
        /// Initializes a new instance of the BsonBinaryData class.
        /// </summary>
        /// <param name="bytes">The binary data.</param>
        /// <param name="subType">The binary data subtype.</param>
        /// <param name="guidRepresentation">The representation for Guids.</param>
        [Obsolete("This constructor will be removed in a later release.")]
        public BsonBinaryData(byte[] bytes, BsonBinarySubType subType, GuidRepresentation guidRepresentation)
            : this(bytes, subType)
        {
            if (BsonDefaults.GuidRepresentationMode != GuidRepresentationMode.V2)
            {
                throw new InvalidOperationException("BsonBinaryData constructor that takes a GuidRepresentation can only be used when GuidRepresentationMode is V2.");
            }

            if (subType == BsonBinarySubType.UuidStandard || subType == BsonBinarySubType.UuidLegacy)
            {
                BsonBinarySubType expectedSubType;
                switch (guidRepresentation)
                {
                    case GuidRepresentation.CSharpLegacy:
                    case GuidRepresentation.JavaLegacy:
                    case GuidRepresentation.PythonLegacy:
                    case GuidRepresentation.Unspecified:
                        expectedSubType = BsonBinarySubType.UuidLegacy;
                        break;

                    case GuidRepresentation.Standard:
                        expectedSubType = BsonBinarySubType.UuidStandard;
                        break;

                    default:
                        throw new ArgumentException($"Invalid guidRepresentation: {guidRepresentation}.", nameof(guidRepresentation));
                }

                if (subType != expectedSubType)
                {
                    throw new ArgumentException($"GuidRepresentation {guidRepresentation} is only valid with subType {expectedSubType}, not with subType {subType}.", nameof(guidRepresentation));
                }
            }
            else
            {
                if (guidRepresentation != GuidRepresentation.Unspecified)
                {
                    var message = string.Format(
                        "GuidRepresentation must be Unspecified, not {0}, when subType is not UuidStandard or UuidLegacy.",
                        guidRepresentation);
                    throw new ArgumentException(message, nameof(guidRepresentation));
                }
            }

            _guidRepresentation = guidRepresentation;
        }

        /// <summary>
        /// Initializes a new instance of the BsonBinaryData class.
        /// </summary>
        /// <param name="guid">A Guid.</param>
#pragma warning disable 618
        [Obsolete("Use the constructor that also takes a GuidRepresentation instead.")]
        public BsonBinaryData(Guid guid)
            : this(
                guid,
                BsonDefaults.GuidRepresentationMode == GuidRepresentationMode.V2
                    ? BsonDefaults.GuidRepresentation
                    : throw new InvalidOperationException("This constructor can only be used when BsonDefaults.GuidRepresentationMode is V2."))
        {
        }
#pragma warning restore 618

        /// <summary>
        /// Initializes a new instance of the BsonBinaryData class.
        /// </summary>
        /// <param name="guid">A Guid.</param>
        /// <param name="guidRepresentation">The representation for Guids.</param>
        public BsonBinaryData(Guid guid, GuidRepresentation guidRepresentation)
        {
            _bytes = GuidConverter.ToBytes(guid, guidRepresentation);
            _subType = GuidConverter.GetSubType(guidRepresentation);
#pragma warning disable 618
            if (BsonDefaults.GuidRepresentationMode == GuidRepresentationMode.V2)
            {
                _guidRepresentation = guidRepresentation;
            }
#pragma warning restore 618
        }

        // public properties
        /// <summary>
        /// Gets the BsonType of this BsonValue.
        /// </summary>
        public override BsonType BsonType
        {
            get { return BsonType.Binary; }
        }

        /// <summary>
        /// Gets the binary data.
        /// </summary>
        public byte[] Bytes
        {
            get { return _bytes; }
        }

        /// <summary>
        /// Gets the representation to use when representing the Guid as BSON binary data.
        /// </summary>
        [Obsolete("This property will be removed in a later release.")]
        public GuidRepresentation GuidRepresentation
        {
            get
            {
                if (BsonDefaults.GuidRepresentationMode != GuidRepresentationMode.V2)
                {
                    throw new InvalidOperationException("GuidRepresentation property can only be used when GuidRepresentationMode is V2.");
                }
                return _guidRepresentation;
            }
        }

        /// <summary>
        /// Gets the BsonBinaryData as a Guid if the subtype is UuidStandard or UuidLegacy, otherwise null.
        /// </summary>
#pragma warning disable 618 // about obsolete BsonBinarySubType.OldBinary
        [Obsolete("Use Value instead.")]
        public override object RawValue
        {
            get
            {
                if (_subType == BsonBinarySubType.Binary || _subType == BsonBinarySubType.OldBinary)
                {
                    return _bytes;
                }
                else if (_subType == BsonBinarySubType.UuidStandard || _subType == BsonBinarySubType.UuidLegacy)
                {
                    return ToGuid();
                }
                else
                {
                    return null;
                }
            }
        }
#pragma warning restore 618

        /// <summary>
        /// Gets the binary data subtype.
        /// </summary>
        public BsonBinarySubType SubType
        {
            get { return _subType; }
        }

        // public operators
        /// <summary>
        /// Converts a byte array to a BsonBinaryData.
        /// </summary>
        /// <param name="bytes">A byte array.</param>
        /// <returns>A BsonBinaryData.</returns>
        public static implicit operator BsonBinaryData(byte[] bytes)
        {
            return new BsonBinaryData(bytes);
        }

        /// <summary>
        /// Converts a Guid to a BsonBinaryData.
        /// </summary>
        /// <param name="value">A Guid.</param>
        /// <returns>A BsonBinaryData.</returns>
        [Obsolete("Use the BsonBinaryData constructor instead and specify a Guid representation.")]
        public static implicit operator BsonBinaryData(Guid value)
        {
            if (BsonDefaults.GuidRepresentationMode != GuidRepresentationMode.V2)
            {
                throw new InvalidOperationException("Implicit conversion from Guid to BsonBinaryData is only valid when BsonDefaults.GuidRepresentationMode is V2.");
            }
            return new BsonBinaryData(value);
        }

        /// <summary>
        /// Compares two BsonBinaryData values.
        /// </summary>
        /// <param name="lhs">The first BsonBinaryData.</param>
        /// <param name="rhs">The other BsonBinaryData.</param>
        /// <returns>True if the two BsonBinaryData values are not equal according to ==.</returns>
        public static bool operator !=(BsonBinaryData lhs, BsonBinaryData rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two BsonBinaryData values.
        /// </summary>
        /// <param name="lhs">The first BsonBinaryData.</param>
        /// <param name="rhs">The other BsonBinaryData.</param>
        /// <returns>True if the two BsonBinaryData values are equal according to ==.</returns>
        public static bool operator ==(BsonBinaryData lhs, BsonBinaryData rhs)
        {
            if (object.ReferenceEquals(lhs, null)) { return object.ReferenceEquals(rhs, null); }
            return lhs.Equals(rhs);
        }

        // public static methods
        /// <summary>
        /// Creates a new BsonBinaryData.
        /// </summary>
        /// <param name="value">An object to be mapped to a BsonBinaryData.</param>
        /// <returns>A BsonBinaryData or null.</returns>
        public new static BsonBinaryData Create(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return (BsonBinaryData)BsonTypeMapper.MapToBsonValue(value, BsonType.Binary);
        }

        // public methods
        /// <summary>
        /// Compares this BsonBinaryData to another BsonBinaryData.
        /// </summary>
        /// <param name="other">The other BsonBinaryData.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonBinaryData is less than, equal to, or greather than the other.</returns>
        public int CompareTo(BsonBinaryData other)
        {
            if (other == null) { return 1; }
            int r = _subType.CompareTo(other._subType);
            if (r != 0) { return r; }
            for (int i = 0; i < _bytes.Length && i < other._bytes.Length; i++)
            {
                r = _bytes[i].CompareTo(other._bytes[i]);
                if (r != 0) { return r; }
            }
            return _bytes.Length.CompareTo(other._bytes.Length);
        }

        /// <summary>
        /// Compares the BsonBinaryData to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonBinaryData is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            if (other == null) { return 1; }
            var otherBinaryData = other as BsonBinaryData;
            if (otherBinaryData != null)
            {
                return CompareTo(otherBinaryData);
            }
            return CompareTypeTo(other);
        }

        /// <summary>
        /// Compares this BsonBinaryData to another BsonBinaryData.
        /// </summary>
        /// <param name="rhs">The other BsonBinaryData.</param>
        /// <returns>True if the two BsonBinaryData values are equal.</returns>
        public bool Equals(BsonBinaryData rhs)
        {
            if (object.ReferenceEquals(rhs, null) || GetType() != rhs.GetType()) { return false; }
            // note: guidRepresentation is not considered when testing for Equality
            return object.ReferenceEquals(this, rhs) || _subType == rhs._subType && _bytes.SequenceEqual(rhs._bytes);
        }

        /// <summary>
        /// Compares this BsonBinaryData to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a BsonBinaryData and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BsonBinaryData); // works even if obj is null or of a different type
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            // note: guidRepresentation is not considered when computing the hash code
            int hash = 17;
            hash = 37 * hash + Hasher.GetHashCode(BsonType);
            foreach (byte b in _bytes)
            {
                hash = 37 * hash + b;
            }
            hash = 37 * hash + Hasher.GetHashCode(_subType);
            return hash;
        }

        /// <summary>
        /// Converts this BsonBinaryData to a Guid.
        /// </summary>
        /// <returns>A Guid.</returns>
        public Guid ToGuid()
        {
#pragma warning disable 618
            if (BsonDefaults.GuidRepresentationMode == GuidRepresentationMode.V2)
            {
                return ToGuid(_guidRepresentation);
            }
            else
            {
                if (_subType != BsonBinarySubType.UuidStandard)
                {
                    throw new InvalidOperationException("ToGuid without a Guid representation can only be called when sub type is UuidStandard.");
                }
                return GuidConverter.FromBytes(_bytes, GuidRepresentation.Standard);
            }
#pragma warning restore 618
        }

        /// <summary>
        /// Converts this BsonBinaryData to a Guid.
        /// </summary>
        /// <param name="guidRepresentation">The representation for Guids.</param>
        /// <returns>A Guid.</returns>
        public Guid ToGuid(GuidRepresentation guidRepresentation)
        {
            if (_subType != BsonBinarySubType.UuidStandard && _subType != BsonBinarySubType.UuidLegacy)
            {
                var message = string.Format("SubType must be UuidStandard or UuidLegacy, not {0}.", _subType);
                throw new InvalidOperationException(message);
            }
            if (guidRepresentation == GuidRepresentation.Unspecified)
            {
                throw new ArgumentException("GuidRepresentation cannot be Unspecified.");
            }
#pragma warning disable 618
            if (BsonDefaults.GuidRepresentationMode == GuidRepresentationMode.V3)
            {
                var expectedSubType = GuidConverter.GetSubType(guidRepresentation);
                if (_subType != expectedSubType)
                {
                    throw new InvalidOperationException($"ToGuid with Guid representation {guidRepresentation} can only be called when sub type is {expectedSubType}.");
                }
            }
#pragma warning restore 618

            return GuidConverter.FromBytes(_bytes, guidRepresentation);
        }

        /// <summary>
        /// Returns a string representation of the binary data.
        /// </summary>
        /// <returns>A string representation of the binary data.</returns>
        public override string ToString()
        {
            return string.Format("{0}:0x{1}", _subType, BsonUtils.ToHexString(_bytes));
        }
    }
}
