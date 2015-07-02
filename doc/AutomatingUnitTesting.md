﻿# Automating unit testing with StatePrinter

This document focuses on how to use StatePrinter to improve the speed with which you flesh out, and maintain, your unit tests. Whereas [The problems with traditional unit testing](TheProblemsWithTraditionalUnitTesting.md) highlights the problems many tests, that do not use StatePrinter, suffer from.

> We honour the dead for giving us the world we inherited, however, we must recognize we are doomed if we allow the dead to govern us.

# Table of Content
 * [Introduction](#introduction)
 * [1. Strategy 1: Manual asserting with automatic consolidation](#1-strategy-1-manual-asserting-with-automatic-consolidation)
   * [1.1 Creating a test](#11-creating-a-test)
   * [1.2 Eventually the business code changes...](#12-eventually-the-business-code-changes)
   * [1.3 Instruct the use of automatic re-writing](#13-instruct-the-use-of-automatic-re-writing)
   * [1.4 Inspect and commit](#14-inspect-and-commit)
 * [2. Strategy 2: Semi-automatic testing](#2-strategy-2-semi-automatic-testing)
   * [2.1 Create the test](#21-create-the-test)
   * [2.2 Run the test](#22-run-the-test)
   * [2.3 Copy-paste the generated asserts](#23-copy-paste-the-generated-asserts)
   * [2.4 Inspect and commit](#24-inspect-and-commit)
     * [Short cut helpers](#short-cut-helpers)
     * [Conclusions](#conclusions)
 * [3. Strategy 3: Full automatic unit tests](#3-strategy-3-full-automatic-unit-tests)
   * [3.1. Instruct StatePrinter to allow auto-rewriting](#31-instruct-stateprinter-to-allow-auto-rewriting)
   * [3.2 Create the test](#32-create-the-test)
   * [3.3 Run the test](#33-run-the-test)
   * [3.4 Inspect and commit](#34-inspect-and-commit)
 * [4. Assert methods: AreAlike, AreEquals, PrintAlike, PrintEquals](#4-assert-methods-arealike-areequals-printalike-printequals)
   * [4.1 `AreEquals()`, `PrintEquals()`](#41-areequals-printequals)
   * [4.2 `AreAlike()`, `PrintAlike()`](#42-arealike-printalike)
   * [4.3 But wait... There is more!](#43-but-wait-there-is-more)
   * [Conclusions](#conclusions)
 * [5. Integrating with your unit testing framework](#5-integrating-with-your-unit-testing-framework)
 * [6. Configuration](#6-configuration)
   * [6.1 Configuring the error message](#61-configuring-the-error-message)
   * [6.2 Restricting fields harvested](#62-restricting-fields-harvested)
   * [6.3 Filtering by use of Types](#63-filtering-by-use-of-types)
 * [7. Best practices](#7-best-practices)
   * [7.1 StatePrinter configuration](#71-stateprinter-configuration)
   * [7.2 Asserting with `AreAlike()` and `AreEquals()`.](#72-asserting-with-arealike-and-areequals)
   * [7.3 Smoother automatic rewrite control](#73-smoother-automatic-rewrite-control)


# Introduction

This document describes **three different strategies** for leveraging StatePrinter for optimal unit test productivity. The strategies introduced here are presented in "order of familiarity", thus we start out "slow" and let things get more insane as we go along. Hold on to your hats and make sure to read all strategies presented before you make up your mind with regards to the strategy that best suits your project or organization. 

This document explains a radically different approach to writing and maintaining asserts in unit tests. Read with an open mind.




# 1. Strategy 1: Manual asserting with automatic consolidation

The straight forward way to use Stateprinter is, to only use it for generating values for your asserts. In turn, this will enable to you to quickly rectify your asserts when inner logic of your business code changes. For the most part, it's business as usual and we have not really solved the problems with unit testing presented in [The problems with traditional unit testing](TheProblemsWithTraditionalUnitTesting.md). 

To a large degree, I see this way of using Stateprinter, as a way to introduce Stateprinter to your project/organization. You don't rock the boat too much, yet you are paving the way for further automation.  

The work flow is as follows

1. Write business code as usual
2. Write tests using Stateprinter for generating the values of the asserts
3. Run the test until they are *green*.
4. Eventually business code changes, rendering some  tests *red*.
5. Instruct Stateprinter to use auto-rewriting of unit test and issue a run of all unit tests
6. Before commit, inspect the changes to both the business code *and tests*.

The only real surprise here should be #5 - that we don't need to change the test when the code changes. How can this ever work? Allow me to assume you know how to write your business code.


## 1.1 Creating a test 
 
For example writing a test for some simple business code using Nunit:

```C#
[Test]
public void TestProcessOrder()
{
    var printer = CreatePrinter();

    var sut = new OrderProcessor(a, b);
    var actual = sut.Process(c, d);

    Assert.AreEqual("1", printer.PrintObject(actual.OrderNumber));
    Assert.AreEqual("X-mas present", printer.PrintObject(actual.OrderDescription));
    Assert.AreEqual("43", printer.PrintObject(actual.Total));
}
```

Now no one in their right mind would see all that typing as an improvement. And really, you shouldn't have to since we already created a helper method in Stateprinter abstracting away all the typing.

```C#
[Test]
public void TestProcessOrderImproved()
{
    var sut = new OrderProcessor(a, b);
    var actual = sut.Process(c, d);

    var assert = CreatePrinter().Assert;
    assert.PrintEquals("1", actual.OrderNumber);
    assert.PrintEquals("X-mas present", actual.OrderDescription);
    assert.PrintEquals("43", actual.Total);
}
```

Although it may not look like it, we are still using Nunit as our test framework infrastructure. `Stateprinter.PrintEquals()` relies on an existing unit testing framework, typically Nunit, Xunit, etc. You may wonder *what that `CreatePrinter()` business is all about?* We recommend using a single source from which all unit testing Stateprinter instances are created. So let's go ahead and create a Stateprinter instance and bridge it to Nunit's Assert method (how to integrate with your favourite testing framework is detailed a bit further down). Additionally, we change how string values are printed (to omit the surrounding `"`).

```C#
static class Helper
{
    public static Stateprinter CreatePrinter()
    { 
        var printer = new Stateprinter();
        printer.Configuration
            .Test.SetAreEqualsMethod(NUnit.Framework.Assert.AreEqual);
            .Add(new StringConverter(""));
            
        return printer;
    }
}
```
 

## 1.2 Eventually the business code changes...
 
As time passes by, requirements change. Eventually, so does the business code. Without going into too much detail, lets say for the sake of the discussion, that the total of the order changes from `43` to `42`. Naturally, this renders our test red. Now there could be many test asserting on the `Total`. 

So rather than taking on this laborious endeavour of fixing the tests manually, we turn to StatePrinter's ability to automatically rewrite.
 
 
## 1.3 Instruct the use of automatic re-writing

As of now we really haven't accomplished much. Seemingly, all we've done is to change asserts into operating on strings. And rightfully so. Because that is all we have achieved. So far. 

However, we can easily gain a jaw-dropping productivity boost by instructing Stateprinter to re-write the expected results of our unit tests when the business code changes. Later in the other strategies, even more automation can be achieved. 

The rewrite mechanism is as follows. Tests are executed through Stateprinter (e.g. using the `PrintEquals()`), who in turn calls the underlying testing framework the for the detailed error reporting (and which your infrastructure such as build server relies on). Stateprinter knows when the expected and actual values deviate, and it knows the deviation. Automatic re-write then simply is a matter of intelligently performing a search-and-replace in the unit test source code file.

We achieve the goodies by adding to the configuration in `CreatePrinter()`:

```C#
printer.Configuration.Test.SetAutomaticTestRewrite(filename => true)
```

Which means that, for any unit test filename, a rewrite of the source code is allowed. Now this is very aggressive and just the thought of automatic rewrite may scare you a little. That's why we in the [7. Best practices](#7.best-practices) section suggest you turn rewriting on/off using your shell.

When the automatic rewrite is allowed, running the unit tests will change the test into

```C#
[Test]
public void TestProcessOrderImproved()
{
    var sut = new OrderProcessor(a, b);
    var actual = sut.Process(c, d);

    var assert = CreatePrinter().Assert;
    assert.PrintEquals("1", actual.OrderNumber);
    assert.PrintEquals("X-mas present", actual.OrderDescription);
    assert.PrintEquals("42", actual.Total);
}
```

Notice how the assert `actual.Total` is rewritten. 
 
A problem with this approach, however, is that only the values of the asserts can be rewritten. Say that our imaginary change of business requirement was that the `Total` should be split into a `Total`and a `VAT Total`. This would require our test to get extended with yet another assert. With the current strategy, that is a manual procedure. Using the other strategies, this is an automated process.
 
 
## 1.4 Inspect and commit

*"Now hold on a minute"*, I hear you say! You just generated the new asserts. How does StatePrinter know that the assert is correct? The answer is: It doesn't. StatePrinter doesn't leave out the necessity to think. In fact you should not blindly trust the output. Just the same as when you blindly re-run your failing tests changing them assert-by-assert until they are green again (yes this happens on large enterprise systems so big no one knows all the lines of code). **StatePrinter simply takes away the typing when correcting tests.**


 

 
# 2. Strategy 2: Semi-automatic testing

The work flow is as follows

1. Create a test, with an empty expected (or change existing code that has tests)
2. Run the tests
3. Copy-paste test expectations from the StatePrinter error message to the test
4. Before commit, inspect the changes to both the business code *and the tests*.

 
## 2.1 Create the test

To get started with automatic asserting in unit testing, you first write your business code and an *empty test* which only exercise the code under test. 

Remember, no asserts are to be written or maintained manually any more. 

The below situation is comparable to having unit tests for business code that has changed, since last running the tests (i.e. they will fail upon execution).

For example:

```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b, ...);

    var printer = Helper.CreatePrinter();
    var actual = printer.PrintObject(sut.ProcessOrder(c, d, ...));
    
    var expected = "";
    printer.Assert.AreEquals(expected, actual);    
}
```



## 2.2 Run the test
Running the test will *fail*. This is quite intentional! Notice the text at the top of the error message below, it says *Proposed output*. This is in fact **proposed C# code, that you can paste directly into your test to make it green**. 

```C#
Proposed output for unit test:
var expected = @"new Order()
{
    OrderNo = 1
    OrderName = ""X-mas present""
    Total = 43
}
";

  Expected string length 0 but was 127. Strings differ at index 0.
  Expected: <string.Empty>
  But was:  "new Order()\r\n{\r\n    OrderNo = 1\r\n    ..."
  -----------^
```


## 2.3 Copy-paste the generated asserts

Copy-paste the `expected` definition into your test. I've taken the liberty to replace `AreEquals()` with `AreAlike()`. For now, suffice to say that `AreAlike()` is a more lenient. 

Your test now looks like:

```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b, ...);

    var printer = Helper.CreatePrinter();
    var actual = printer.PrintObject(sut.ProcessOrder(c, d, ...));
    
    var expected = @"new Order()
    {
       OrderNo = 1
       OrderName = ""X-mas present""
       Total = 43
    }";
    printer.Assert.AreAlike(expected, actual);    
}
```

## 2.4 Inspect and commit

*"Now hold on a minute"*, I hear you say! You just generated the output. How does StatePrinter know that the assert is correct? The answer is: It doesn't. It merely outputs the state of the SUT (System Under Test). So StatePrinter does not prevent you from having to think. In fact you should not blindly trust the output. **StatePrinter simply takes away the typing in testing.**


### Short cut helpers

Now that we understand the basics of the framework, it is time to introduce a shorthand method for printing and asserting in one go. This keeps typing at a minimum. We can re-write the above test simply as:


```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b, ...);
    
    var expected = "";
    Helper.CreatePrinter().Assert.PrintAreAlike(expected, sut.ProcessOrder(c, d, ...));    
}
```

### Conclusions

* You get the assert creation for free
* When the order-object is extended in the future, your unit test failure message will tell you how to go green again. 
 
If you don't like the output format of the `expected` variable, read  [configuration](HowToConfigure.md) for heaps of ways to tweak.







# 3. Strategy 3: Full automatic unit tests

The work flow is as follows

1. Instruct StatePrinter to allow auto-rewriting
2. Create a test, with an empty expected (or change existing code that has tests)
3. Run the test
4. Before commit, inspect the changes to both the business code *and the tests*.

This is a much simpler work flow than the semi-automatic approach since it does not involve any copy and pasting. You simply run your tests and they conform to the code. This is because StatePrinter will search/replace directly in your source code.

Naturally, this is not suitable for all development situations. But very often  I find myself in the situation, where a simple code change requires significant time to fix-up several existing tests. Often times, these tests are integration and/or acceptance tests. 



## 3.1. Instruct StatePrinter to allow auto-rewriting

Simply extend the above `Helper.CreatePrinter()` with 

```C#
printer.Configuration.SetAutomaticTestRewrite(fileName => true);
```

This instructs Stateprinter to automatically re-write expected values in the source code of the unit test, to reflect changes in the business code. The configuration above is quite liberal in allowing this rewrite to occur for any unit test file executed. Notice, that this is just one way to do the configuration. See section "best practices" for a superior approach, that take a little more explaining.

 
## 3.2 Create the test

Create a new test or think of this as one of your existing tests where the business code has changed since last running this test.

```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b, ...);

    var printer = Helper.CreatePrinter();
    var actual = printer.PrintObject(sut.Foo(c, d, ...));
    
    var expected = "";
    printer.Assert.AreEquals(expected, actual);    
}
```

## 3.3 Run the test

From within visual studio or using an external GUI.

The tests will go red, but the error message will notify you that the test has been re-written and thus upon re-execution will go green. If you refresh in Visual Studio or whatever editor, your unit test source code has changed such that the `expected` variable now holds the result from executing `sut.Foo()`.

```C#
[Test]
public void MakeOrderTest()
{ 
    var sut = new BusinessCode(a, b);

    var printer = Helper.CreatePrinter();
    var actual = printer.PrintObject(sut.Foo(c, d));
    
    var expected = @"new Order()
    {
       OrderNo = 1
       OrderName = ""X-mas present""
       Total = 43
    }";
    printer.Assert.AreEquals(expected, actual);    
}
```


Notice how the assert `actual.Total` is rewritten. 
 
Different to the "Strategy 1" presented above, that should the number of fields of the order object change, such changes are automatically reflected in the rewrite. This is possible since the whole assert is formulated in terms of strings rather than C# code (and Stateprinter is really good at generating strings :-)
 
 
 
## 3.4 Inspect and commit

*"Now hold on a minute"*, I hear you say! You just generated the output. How does StatePrinter know that the assert is correct? The answer is, that it doesn't. It merely outputs the state of the SUT (System Under Test). So StatePrinter does not prevent you from having to think, in fact you should not blindly trust the output. StatePrinter simply takes away the typing in testing.





# 4. Assert methods: AreAlike, AreEquals, PrintAlike, PrintEquals

We have now concluded the various strategies in which Stateprinter can be used in leveraging your unit tests. Occasionally, we have presented different assert methods. Now let's take the full tour of asserts. Generally, the assert methods hold the following properties

* They are accessible from `printer.Assert.`
* They wrap the current unit testing framework of your choice 
* They code generate your expected values. It is almost fully automatic to write your asserts and update them when the code changes.
* Some of them are lenient to newline issues, by unifying the line ending representation before asserting.


## 4.1 `AreEquals()`, `PrintEquals()`

The "equals methods" compare byte for byte that the content of `expected` and `actual` are the same.

Usage of `AreEquals()`

```C#
var printer = Helper.CreatePrinter();
var actual = printer.PrintObject(sut.ProcessOrder(c, d));

var expected = "bla bla bla";
printer.Assert.AreEquals(expected, actual); 
```    

A shorter way to express this is to *combine the PrintObject with the AreEquals*. This is what the `PrintEquals()` is for.

Usage of `PrintEquals()`

```C#
var printer = Helper.CreatePrinter();

var expected = "bla bla bla";
printer.Assert.PrintEquals(expected, sut.ProcessOrder(c, d)); 
```    


## 4.2 `AreAlike()`, `PrintAlike()`

The "alike methods" first modifies the line endings of the `expected` and `actual` before passing on the values to the equals method. The modifications is a "unification" that ensures that differences only in `\r\n` and `\r` and `\n` between expected and actual, are ignored. 

These assert methods are are particularly nice when you are coding and testing on multiple operating systems (such as deploying to the cloud, e.g. AppVeyor, AppHabour) or when you face problems when copy/pasting the generated new expected values (as shown in strategy 2).


Usage of `AreAlike()`

```C#
var printer = Helper.CreatePrinter();
var actual = printer.PrintObject(sut.ProcessOrder(c, d));

var expected = "bla bla bla";
printer.Assert.AreAlike(expected, actual); 
```    

A shorter way to express this is to *combine the PrintObject with the AreAlike*. This is what the `PrintAlike()` is for.

Usage of `PrintAlike()`

```C#
var printer = Helper.CreatePrinter();

var expected = "bla bla bla";
printer.Assert.PrintAlike(expected, sut.ProcessOrder(c, d)); 
```    

## 4.3 But wait... There is more!

Support for the more fluent assert api has been provided. E.g. you are able to use `That(expected, It.IsEqualTo(actual))`. See further details in the source code https://github.com/kbilsted/StatePrinter/blob/master/StatePrinter/TestAssistance/Asserter.cs 


## Conclusions

When not using automatic rewrite, I prefer the `AreAlike()` over the `AreEquals()`. 

With automatic rewrite, there are no issues with copy-pasting, and thus it is safer to use the AreEqual variant which does not modify line endings before comparison.






# 5. Integrating with your unit testing framework

Stateprinter is not dependent on any particular unit testing framework, in fact it will integrate with most if not all frameworks on the market. This is possible through explicit configuration where you tell how StatePrinter must call your testing frameworks' assert method. For Nunit the below suffices:

```C#
var printer = new Stateprinter();
printer.Configuration.Test.SetAreEqualsMethod( Nunit.Framework.Assert.AreEquals );
```

and similar for other frameworks, you simply just have to tell Stateprinter the equals method you are using.


# 6. Configuration 

## 6.1 Configuring the error message

The error message shown when a test is failing is fully configurable. This enables to cater for specific needs such as fully printing the content of the actual and expected values. Use

```C#
printer.Configuration.Test.SetAssertMessageCreator( ... );
```

For inspiration grok the default implementation at [StatePrinter/TestAssistance/DefaultAssertMessage.cs](DefaultAssertMessage.cs)



## 6.2 Restricting fields harvested

Now, there are situations where there are fields in your business objects that are uninteresting for your tests. Thus those fields represent a challenge to your test. 

* They may hold uninteresting values polluting the assert, e.g. a `Guid`
* They may change value from execution to execution, e.g. `DateTime.Now`

We can easily remedy this situation using the FieldHarvester abstraction described in the "configuration document", however, we do not feel inclined to create an implementation of the harvesting interface per class to be tested. The `ProjectiveHarvester` has a wealth of possibilities to transform (project) a type into another. That is, only include certain fields, only exclude certain fields, or create a filter programmatically. 

given

```C#
class A
{
   public DateTime X;
   public DateTime Y { get; set; }
   public string Name;
}
```

You can *in a type safe manner, and using auto-completion of visual studio* include or exclude fields. Notice that the type is provided in the call (`A`) therefore the editor can help suggest which properties or fields to include or exclude. Unlike the normal field-harvester, the `ProjectiveHarvester` uses the FieldsAndProperties field harvester so it will by default include more than what you might be used to from using the normal field processor.

```C#
Asserter assert = TestHelper.CreateShortAsserter();
assert.Project.Exclude<A>(x => x.X, x => x.Y);

var state = new A { X = DateTime.Now, Name = "Charly" };
assert.PrintEquals("new A(){ Name = ""Charly""}", state);
```

alternatively, we can just mention the fields we'd like to see:

```C#
Asserter assert = TestHelper.CreateShortAsserter();
assert.Project.Include<A>(x => x.Name);

var state = new A { X = DateTime.Now, Name = "Charly" };
assert.PrintEquals("new A(){ Name = ""Charly""}", state);
```

or programatically

```C#
Asserter assert = TestHelper.CreateShortAsserter();
assert.Project.AddFilter<A>(x => x.Where(y => y.SanitizedName != "X" && y.SanitizedName != "Y"));
```

You can now easily configure what to dump when testing. 

Notice though, that when you use the `Include` or `AddFilter` functionality, you are excluding yourself from failing tests when your business data is extended. So use it with care.



## 6.3 Filtering by use of Types

As of v2.1 you can specify filters by using other types. Say you have a class implementing multiple interfaces, you can specify to only include fields from specific interface(s).

```C#
[Test]
public void TestIncludeByType()
{
     var sut = new AtoD();
     Asserter assert;

     assert = TestHelper.CreateShortAsserter();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.IncludeByType<AtoD, IA>();
     assert.PrintEquals("new AtoD() { A = 1 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.IncludeByType<AtoD, IA, IB>();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.IncludeByType<AtoD, IA, IB, IC>();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.IncludeByType<AtoD, IA, IB, IC, ID>();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);
 }

 [Test]
 public void TestExcludeByType()
 {
     var sut = new AtoD();
     Asserter assert;

     assert = TestHelper.CreateShortAsserter();
     assert.PrintEquals("new AtoD() { A = 1 B = 2 C = 3 D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.ExcludeByType<AtoD, IA>();
     assert.PrintEquals("new AtoD() { B = 2 C = 3 D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.ExcludeByType<AtoD, IA, IB>();
     assert.PrintEquals("new AtoD() { C = 3 D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.ExcludeByType<AtoD, IA, IB, IC>();
     assert.PrintEquals("new AtoD() { D = 4 }", sut);

     assert = TestHelper.CreateShortAsserter();
     assert.Project.ExcludeByType<AtoD, IA, IB, IC, ID>();
     assert.PrintEquals("new AtoD() { }", sut);
 }
```



# 7. Best practices

## 7.1 StatePrinter configuration

The bast practices when using StatePrinter for unit testing is different from using it to do ToString-implementations. The caching of field harvesting of types is not compatible with using the `Include<>`, `Exclude<>` and filter properties of the `ProjectionHarvester`. Thus to ensure correctness of our tests and to ensure that our tests are independent of each other, the best strategy is to use a fresh instance of StatePrinter in each test.

There are many ways of achieving this, but I found the best way to use a `TestHelper` class that provides general configuration shared among all tests, and then for specific tests, tune this configuration. Eg. with filters.

Things to consider in the general case

* The use of a specific culture to ensure that your tests work across platforms. For example, AppVeyor has a completely different setup than my local machine.
* Setup type converters as you want them. E.g. do you want strings in the output to be enclosed in `""` or maybe you have attributes on your enum-values that you want to see in the output.
* Hook up your unit testing framework 

You can come a long way with the following code

```C#
static class Create
{
    public static Asserter Asserter()
    {
        var cfg = ConfigurationHelper.GetStandardConfiguration()
            .SetCulture(CultureInfo.CreateSpecificCulture("da-DK"))
            .Test.SetAreEqualsMethod(NUnit.Framework.Assert.AreEqual)
            .Test.SetAutomaticTestRewrite((fileName) => true);

            return new Stateprinter(cfg).Assert;
    }
}
```

Then you can use, and further fine-tune your test like

```C#
[Test]
public void Foo()
{
    var assert = Create.Asserter();
    assert.Configuration  ... // fine tuning

    assert.PrintAreEquals(...);
```



## 7.2 Asserting with `AreAlike()` and `AreEquals()`.

When not using automatic rewrite, I prefer the `AreAlike()` over the `AreEquals()`. I've come to really appreciate the `AreAlike()` method since it ignores differences in line endings. Line endings differ from operating system to operating system, and some tools such as ReSharper seems to have problems when copying from its output window into tests. Here the line endings are truncated to `\n`. 

With automatic rewrite, there are no issues with copy-pasting, and thus it is safer to use the AreEqual variant which does not modify line endings before comparison.



## 7.3 Smoother automatic rewrite control

In the above example, we configure the automatic rewrite editing the code, for the `SetAutomaticTestRewrite()` call. This is both a bit cumbersome, and has the disadvantage, that you may accidentally commit setting automatic rewrite to `true`. A smoother solution is to use an environment variable to turn on/off the automatic rewrites. You can then edit this variable through your shell. You can achieve this with the following configuration: 

```C#
// requires stateprinter v2.1.xx
configuration.Test.SetAutomaticTestRewrite(x => new EnvironmentReader().UseTestAutoRewrite());
```
and then use these functions in PowerShell

```PowerShell
function DoAutoRewrite() 
{ 
    [Environment]::SetEnvironmentVariable("StatePrinter_UseTestAutoRewrite", "true", "User")
}

function ForbidAutoRewrite() 
{ 
    [Environment]::SetEnvironmentVariable("StatePrinter_UseTestAutoRewrite", "false", "User")
}
```



Have fun!

Kasper B. Graversen
