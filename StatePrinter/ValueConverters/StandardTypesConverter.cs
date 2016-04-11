// Copyright 2014 Kasper B. Graversen
// 
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using StatePrinting.Configurations;

namespace StatePrinting.ValueConverters
{
    /// <summary>
    /// Handles the printing of
    /// Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, 
    /// Double, Single, decimal, Guid 
    /// </summary>
    public class StandardTypesConverter : IValueConverter
    {
        readonly Configuration configuration;

        public StandardTypesConverter(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public bool CanHandleType(Type t)
        {
            // Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
            if (t.IsPrimitive)
                return true;

            if (t == typeof(decimal) || t == typeof(Guid))
                return true;

            return false;
        }

        public string Convert(object source)
        {
            if (source is float)
                return ((float)source).ToString(configuration.Culture);
            if (source is double)
                return ((double)source).ToString(configuration.Culture);
            if (source is decimal)
                return ((decimal)source).ToString(configuration.Culture);

            return source.ToString();
        }
    }
}