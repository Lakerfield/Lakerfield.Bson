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

namespace Lakerfield.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that sets whether to ignore extra elements encountered during deserialization.
    /// </summary>
    public class IgnoreExtraElementsConvention : ConventionBase, IClassMapConvention
    {
        // private fields
        private bool _ignoreExtraElements;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreExtraElementsConvention" /> class.
        /// </summary>
        /// <param name="ignoreExtraElements">Whether to ignore extra elements encountered during deserialization.</param>
        public IgnoreExtraElementsConvention(bool ignoreExtraElements)
        {
            _ignoreExtraElements = ignoreExtraElements;
        }

        /// <summary>
        /// Applies a modification to the class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void Apply(BsonClassMap classMap)
        {
            classMap.SetIgnoreExtraElements(_ignoreExtraElements);
        }
    }
}
