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

namespace Lakerfield.Bson.Serialization
{
    /// <summary>
    /// An interface implemented by Id generators.
    /// </summary>
    public interface IIdGenerator
    {
        /// <summary>
        /// Generates an Id for a document.
        /// </summary>
        /// <param name="container">The container of the document (will be a MongoCollection when called from the C# driver). </param>
        /// <param name="document">The document.</param>
        /// <returns>An Id.</returns>
        object GenerateId(object container, object document);
        /// <summary>
        /// Tests whether an Id is empty.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>True if the Id is empty.</returns>
        bool IsEmpty(object id);
    }
}
