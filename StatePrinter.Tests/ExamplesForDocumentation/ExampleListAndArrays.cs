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

namespace StatePrinting.Tests.ExamplesForDocumentation
{
    [TestFixture]
    class ExampleEndlessAsserts
    {

        [Test]
        public void ExampleListAndArraysAlternative()
        {
            object products = 1;
            object vendors = 2;
            object year = 2222;
            int added1 = 0;
            int added2 = 0;
            int added3 = 0;
            var vendorManager = new TaxvendorManager(products, vendors, year);
            vendorManager.AddVendor(JobType.JobType1, added1);
            vendorManager.AddVendor(JobType.JobType2, added2);
            vendorManager.AddVendor(JobType.JobType3, added3);

            var expected = @"new VendorAllocation[]()
{
    [0] = new VendorAllocation()
    {
        Allocation = 100
        Price = 20
        Share = 20
    }
    [1] = new VendorAllocation()
    {
        Allocation = 120
        Price = 550
        Share = 30
    }
    [2] = new VendorAllocation()
    {
        Allocation = 880
        Price = 11
        Share = 50
    }
}";

            TestHelper.Assert().PrintAreAlike(expected, vendorManager.VendorJobSplit);
        }
    }


    enum JobType
    {
        JobType3,
        JobType2,
        JobType1
    }

    class TaxvendorManager
    {
        public TaxvendorManager(object products, object vendors, object year)
        {
            VendorJobSplit = new VendorAllocation[]
                                 {
                                     new VendorAllocation(){Allocation = 100, Price = 20, Share = 20f}, 
                                     new VendorAllocation(){Allocation = 120, Price = 550, Share = 30f}, 
                                     new VendorAllocation(){Allocation = 880, Price = 11, Share = 50f}, 
                                 };
        }

        public VendorAllocation[] VendorJobSplit { get; set; }

        public void AddVendor(object jobType3, object added3)
        {
                
        }
    }

    class VendorAllocation
    {
        public int Allocation, Price;
        public float Share;
    }
}
