using System.CodeDom.Compiler;
using Bogus;
using FluentAssertions;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;

namespace Siteswaps.Generator.Test.Components.State;

[TestFixture]
public class FilterTreeTests
{
    private IFilterInformation PatternFilter(int numberOfPasses = 0) => new PatternFilterInformationFaker().RuleFor(x => x.Pattern, () => Enumerable
        .Repeat(Throw.SinglePass, numberOfPasses).ToList()).Generate();
    
    [Test]
    public async Task FilterTree_Can_Remove_Leaf()
    {
        var leaf = new FilterLeaf(PatternFilter());
        var sut = new FilterTree(leaf);
        var result = sut.Remove(leaf);
        await Verify(new FilterTreePrinter().Print(result));
    }    
    
    [Test]
    public async Task FilterTree_Can_Remove_From_And_Node()
    {
        var leaf = new FilterLeaf(PatternFilter(2));
        var and = new AndNode(new FilterLeaf(PatternFilter(1)),leaf,new FilterLeaf(PatternFilter(3)));
        var sut = new FilterTree(and);
        var result = sut.Remove(leaf);
        await Verify(new FilterTreePrinter().Print(result));
    }  
    
    [Test]
    public async Task FilterTree_Can_Remove_From_And_Node_Deeply_Nested()
    {
        var leaf = new FilterLeaf(PatternFilter(2));
        var and = new OrNode(new AndNode(new OrNode(new AndNode(new FilterLeaf(PatternFilter(1)),leaf,new FilterLeaf(PatternFilter(3))))));
        var sut = new FilterTree(and);
        var result = sut.Remove(leaf);
        await Verify(new FilterTreePrinter().Print(result));
    }  
    
    [Test]
    public async Task FilterTree_Can_Remove_From_Or_Node()
    {
        var leaf = new FilterLeaf(PatternFilter(2));
        var and = new OrNode(new FilterLeaf(PatternFilter(1)),leaf,new FilterLeaf(PatternFilter(3)));
        var sut = new FilterTree(and);
        var result = sut.Remove(leaf);
        await Verify(new FilterTreePrinter().Print(result));
    }  
    
    [Test]
    public async Task FilterTree_Can_Add_To_And_Node_Deeply_Nested()
    {
        var leaf = new FilterLeaf(PatternFilter(2));
        var nestedAnd = new AndNode(new FilterLeaf(PatternFilter(1)),new FilterLeaf(PatternFilter(3)));
        var and = new OrNode(new AndNode(new OrNode(nestedAnd)));
        var sut = new FilterTree(and);
        var result = sut.Add(nestedAnd,leaf);
        await Verify(new FilterTreePrinter().Print(result));
    }  

    
    [Test]
    public void Generate_Unique_Key_For_Leaf_With_Nesting()
    {
        var leaf = new FilterLeaf(PatternFilter(2));
        var nestedAnd = new AndNode(new FilterLeaf(PatternFilter(1)),leaf,new FilterLeaf(PatternFilter(3)));
        var and = new OrNode(new AndNode(new OrNode(nestedAnd)));
        var sut = new FilterTree(and);
        var result = sut.GetKey(leaf);
        result.Should().Contain("OR_AND_OR_AND_");
    }  


    private class FilterTreePrinter
    {
        public string Print(FilterTree tree)
        {
            var stringWriter = new StringWriter();
            var sb = stringWriter.GetStringBuilder();
            var indentedBuilder = new IndentedTextWriter(stringWriter, "  ");
            PrintNode(tree.Root, indentedBuilder);
            indentedBuilder.Flush();
            return sb.ToString();
        }

        private void PrintNode(FilterNode? treeRoot, IndentedTextWriter writer)
        {
            treeRoot?.Visit(new TreePrinter(writer));
        }
    }

    private class TreePrinter(IndentedTextWriter writer) : IFilterVisitor<Unit>
    {
        public Unit Visit(AndNode node)
        {
            writer.WriteLine("And");
            writer.Indent++;
            foreach (var child in node.Children)
            {
                ((IFilterVisitor<Unit>)this).Visit(child);
            }
            writer.Indent--;
            return Unit.Value;
        }

        public Unit Visit(OrNode node)
        {
            writer.WriteLine("Or");
            writer.Indent++;
            foreach (var child in node.Children)
            {
                ((IFilterVisitor<Unit>)this).Visit(child);
            }
            writer.Indent--;
            return Unit.Value;
        }

        public Unit Visit(FilterLeaf node)
        {
            writer.WriteLine(node.Filter.Display());
            return Unit.Value;
        }
    }

    private sealed class PatternFilterInformationFaker : Faker<NewPatternFilterInformation>
    {
        public PatternFilterInformationFaker()
        {
            CustomInstantiator(f => new (new List<Throw>(), false, false));
        }
    }
}