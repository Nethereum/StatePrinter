﻿// Copyright 2014 Kasper B. Graversen
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

using NUnit.Framework;

namespace StatePrinting.Tests.IntegrationTests
{
    [TestFixture]
    class InheritanceTest
    {
        [Test]
        public void HarvestSameFieldsInTypAndSubtype()
        {
            B b = new B();
            ((A)b).SomeFieldOnlyInA = 1;
            ((A)b).SameFieldInAB = "A part";
            b.SomeFieldOnlyInB = 2;
            b.SameFieldInAB = "B part";

            var expected = @"new B()
{
    SomeFieldOnlyInA = 1
    SameFieldInAB = ""A part""
    SomeFieldOnlyInB = 2
    SameFieldInAB = ""B part""
}";
            TestHelper.Assert().PrintAreAlike(expected, (B) b);
            TestHelper.Assert().PrintAreAlike(expected, (A) b);
        }


        class A
        {
            public int SomeFieldOnlyInA;
            public string SameFieldInAB;
        }


        class B : A
        {
            public int SomeFieldOnlyInB;
            public new string SameFieldInAB;
        }
    }
}
