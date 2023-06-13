using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TestCaseOrderAttribute : Attribute
    {
        public TestCaseOrderAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }
    }

    public class TestPriorityOrderer : ITestCollectionOrderer
    {
        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            var result = from item in testCollections.ToList()
                let type = (item.TestAssembly.Assembly.GetType(nameof(TestCaseOrderAttribute)) as ReflectionTypeInfo)?.Type
                let attrib = type.GetCustomAttributes(true).OfType<TestCaseOrderAttribute>().FirstOrDefault()
                select new KeyValuePair<int, ITestCollection>(attrib.Priority, item);

            return result.OrderBy(o => o.Key).Select(s => s.Value).AsEnumerable();
        }
    }
}
