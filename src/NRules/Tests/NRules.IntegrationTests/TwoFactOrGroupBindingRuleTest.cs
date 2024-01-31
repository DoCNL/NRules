﻿using System.Diagnostics.CodeAnalysis;
using NRules.Fluent.Dsl;
using NRules.IntegrationTests.TestAssets;
using NRules.Testing;
using Xunit;

namespace NRules.IntegrationTests;

public class TwoFactOrGroupBindingRuleTest : BaseRulesTestFixture
{
    [Fact]
    public void Fire_FactMatchingFirstPartOfOrGroup_FiresOnce()
    {
        //Arrange
        var fact1 = new FactType1 { TestProperty = "Valid Value 1", Value = "Fact1" };

        Session.Insert(fact1);

        //Act
        Session.Fire();

        //Assert
        Verify(x => x.Rule().Fired(Matched.Fact<string>().Callback(x => Assert.Equal("Fact1", x))));
    }

    [Fact]
    public void Fire_FactsMatchingSecondPartOfOrGroup_FiresOnce()
    {
        //Arrange
        var fact2 = new FactType2 { TestProperty = "Valid Value 2", Value = "Fact2" };

        Session.Insert(fact2);

        //Act
        Session.Fire();

        //Assert
        Verify(x => x.Rule().Fired(Matched.Fact<string>().Callback(x => Assert.Equal("Fact2", x))));
    }

    [Fact]
    public void Fire_FactsMatchingBothPartsOfOrGroup_FiresTwice()
    {
        //Arrange
        var fact1 = new FactType1 { TestProperty = "Valid Value 1", Value = "Fact1" };
        var fact2 = new FactType2 { TestProperty = "Valid Value 2", Value = "Fact2" };

        Session.Insert(fact1);
        Session.Insert(fact2);

        //Act
        Session.Fire();

        //Assert
        VerifySequence(s =>
        {
            s.Rule().Fired(Matched.Fact<string>().Callback(x => Assert.Equal("Fact1", x)));
            s.Rule().Fired(Matched.Fact<string>().Callback(x => Assert.Equal("Fact2", x)));
        });
    }

    protected override void SetUpRules(IRulesTestSetup setup)
    {
        setup.Rule<TestRule>();
    }

    public class FactType1
    {
        [NotNull]
        public string? TestProperty { get; set; }
        [NotNull]
        public string? Value { get; set; }
    }

    public class FactType2
    {
        [NotNull]
        public string? TestProperty { get; set; }
        [NotNull]
        public string? Value { get; set; }
    }

    public class TestRule : Rule
    {
        public override void Define()
        {
            FactType1 fact1 = null!;
            FactType2 fact2 = null!;
            string value = null!;

            When()
                .Or(x => x
                    .Match(() => fact1, f => f.TestProperty.StartsWith("Valid"))
                    .Match(() => fact2, f => f.TestProperty.StartsWith("Valid")))
                .Let(() => value, () => GetValue(fact1, fact2));

            Then()
                .Do(ctx => ctx.NoOp());
        }

        private static string GetValue(FactType1? fact1, FactType2? fact2)
        {
            return fact1 != null ? fact1.Value : fact2!.Value;
        }
    }
}