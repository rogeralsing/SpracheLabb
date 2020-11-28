﻿
using PlasticLang;
using PlasticLang.Ast;
using Sprache;
using Xunit;

namespace PlasticSpec
{

    public class UnitTest1
    {
        [Fact]
        public void Can_parse_integer_number()
        {
            var number = PlasticParser.Expression.Parse("  123  ");
        }

        [Fact]
        public void Can_parse_decimal_number()
        {
            var number = PlasticParser.Expression.Parse("  123.456  ");
            Assert.True(number is NumberLiteral);
        }

        [Fact]
        public void Can_parse_identifier()
        {
            var identifier = PlasticParser.Expression.Parse("  abc  ");
            Assert.True(identifier is Symbol);
        }

        [Fact]
        public void Can_parse_identifiers()
        {
            var identifier = PlasticParser.Expression.Parse("  abc def ghi jkl ");
            Assert.True(identifier is ListValue);
        }

        [Fact]
        public void Can_parse_invocation()
        {
            var invocation = PlasticParser.Statement.Parse("  abc def ghi jkl ()  { print(x); } (x) ;  ");
            Assert.True(invocation is ListValue);
        }

        [Fact]
        public void Can_parse_terminating_invocation()
        {
            var invocation = PlasticParser.Statement.Parse("  abc def ghi jkl ()  { print(x); } ");
            Assert.True(invocation is ListValue);
        }

        [Fact]
        public void Can_parse_string()
        {
            var str = PlasticParser.Expression.Parse("  \"hej hopp 12334 !%¤%¤ \"  ");
            Assert.True(str is StringLiteral);
        }

        [Fact]
        public void Can_parse_addition()
        {
            var addition = PlasticParser.Expression.Parse("  a+b  ");
            Assert.True(addition is ListValue);
        }

        [Fact]
        public void Can_parse_subtraction()
        {
            var subtraction = PlasticParser.Expression.Parse("  a-b  ");
            Assert.True(subtraction is ListValue);
        }

        [Fact]
        public void Can_parse_multiplication()
        {
            var multiplication = PlasticParser.Expression.Parse("  a*b  ");
            Assert.True(multiplication is ListValue);
        }

        [Fact]
        public void Can_parse_division()
        {
            var division = PlasticParser.Expression.Parse("  a/b  ");
            Assert.True(division is ListValue);
        }

        [Fact]
        public void Can_parse_assignment()
        {
            var assignment = PlasticParser.Expression.Parse("   a:=2  ");
            Assert.True(assignment is ListValue);
        }

        [Fact]
        public void Can_parse_assignment_assignment()
        {
            var assignment = PlasticParser.Expression.Parse("  a:=b:=c  ");
            Assert.True(assignment is ListValue);
        }
        

        [Fact]
        public void Can_parse_assignment_lambda()
        {
            var assignment = PlasticParser.Expression.Parse("   a := x => b  ");
            Assert.True(assignment is ListValue);
        }

        [Fact]
        public void Can_parse_lambda_declaration_()
        {
            PlasticParser.Expression.Parse("  () => {y;} ");
            PlasticParser.Expression.Parse("  x => y  ");
            PlasticParser.Expression.Parse("  (x) => {y;} ");
            PlasticParser.Expression.Parse("  (a,b) => y  ");
        }

        [Fact]
        public void Can_parse_body()
        {
            var lambda1 = PlasticParser.Body.Parse(" { x; }");
            //    var lambda2 = PlasticParser.Expression.Parse("  x => y  ");
        }

        [Fact]
        public void Can_parse_empty_body()
        {
            var statement = PlasticParser.Body.Parse(" {  }  ");
            //    var lambda2 = PlasticParser.Expression.Parse("  x => y  ");
        }
        
        
        [Fact]
        public void Can_parse_function()
        {
            var f = PlasticParser.Expression.Parse(@"
f := func(a,b,c)
{
    print('abc '+a+' '+b+' '+c)
}
");
            
            Assert.True(f is ListValue);
        }
    }
    
    /*

b := 3
c := a + b
a := a + 1
print('c = ' + c)
print('a = ' + a)

print ('hello'.GetType().Name.Length)

Console := using (System.Console)
print(Console)
Console.WriteLine('.NET interop!!')

tuple       := ('hello','this','is','a','tuple')
arr         := ['hello','this','is','an','array']
statements  := {'hello','this','is','a','body'}     //this will result in "body" as statements are evaluated directly


print (arr.1)

print ('arr length is ' + (arr.'Length' + 100) )
print ('str length is ' + 'some string'.Length)

closureprint := x => print(x + a)
closureprint('foo')

each(element, arr)
{
    print(element)
}

for (a := 0; a < 10; a ++)
{
    print (a)
}

repeat(3)
{
    print('repeat..')
}

a := 1

if (a == 1)
{
    print ('inside if')
}
elif (a == 3)
{
    print ('inside elif')
}
else
{
    print ('inside else')
}

while (a < 5)
{
     print ('daisy me rollin`')
     a++
}

(x => print('lambda fun ' + x))('yay')

if (true, print('hello'))

f := func(a,b,c)
{
    print('abc '+a+' '+b+' '+c)
}

f(1)(2)(3)

multiply := (x,y) => x*y
double := multiply(2)

print ('88 doubled is ' + double(88))

BeepMixin := mixin
{
    beep := func ()
    {
        print ('beep')
    }
}

Person := class (firstName,lastName)
{
    BeepMixin()
    sayHello := func ()
    {
        print ('Hello {0} {1}',firstName,lastName)
    }
}

john := Person('John','Doe')
jane := Person('Jane','Doe')
john.extra = Person('testing sub object','oh yes')
john.firstName = 'Johnny'

john.beep()

john.sayHello()
jane.sayHello()
john.extra.sayHello()

list = LinkedList()
list.add('first')
list.add('second')
list.add('third')
list.add('last')
list.each(v => {
    print ('lamda ' + v)
})

s = Stack()
s.push(1)
s.push(2)
s.push(3)

print (s.pop())
print (s.pop())
print (s.pop())


bar = 123

print ('testing pattern matching')
switch (bar) {
    case(123) {	print('123')}
    case(888) {	print('888!!!!!!!')}
    case(999) {	print('999')}
    default   { print('no match')}
}

{@body => body()}{print ('strange')}

code = 'print("I am eval!!")'
eval(code)

     */
}