using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using LinFu.AOP.Cecil.Extensions;
using LinFu.AOP.Interfaces;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using SampleLibrary;

namespace InterceptionSample
{
    public class SampleInterceptor : IInterceptor
    {
        public object Intercept(IInvocationInfo info)
        {
            var methodName = info.TargetMethod.Name;
            Console.WriteLine("method '{0}' called", methodName);

            // Replace the input parameter with 42
            var result = info.TargetMethod.Invoke(info.Target, new object[] { 42 });

            return result;
        }
    }

    public class SpeakProvider : BaseMethodReplacementProvider
    {
        protected override bool ShouldReplace(object host, IInvocationInfo context)
        {

            // Only replace the Pay method
            return context.TargetMethod.Name == "Pay" &&
                context.TargetMethod.DeclaringType == typeof(Employee);
        }

        protected override IInterceptor GetReplacement(object host, IInvocationInfo context)
        {
            return new SampleInterceptor();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //testDynamicLibraryRegistry();
            TestMethodBodyReplacementByInstance();
            TestMethodBodyReplacementRegistry();

            return;
        }

        private static void testDynamicLibraryRegistry()
        {
            var targetFile = typeof(Employee).Assembly.Location;
            var assemblyDefinition = AssemblyFactory.GetAssembly(targetFile);

            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(targetFile);
            var pdbFileName = string.Format("{0}.pdb", filenameWithoutExtension);
            var pdbExists = File.Exists(pdbFileName);

            var module = assemblyDefinition.MainModule;

            assemblyDefinition.InterceptAllMethodCalls();

            var outputFile = new MemoryStream();
            assemblyDefinition.Save(outputFile);

            // Load the modified assembly
            var assembly = Assembly.Load(outputFile.ToArray());

            // Hook the SpeakProvider into the method body
            MethodBodyReplacementProviderRegistry.SetProvider(new SpeakProvider());

            // Load the employee type
            var types = assembly.GetTypes();

            var employeeType = types.First(t => t.Name == "Employee");
            dynamic employee = CreateInstance(employeeType);
            employee.Pay(256);
            Employee.SayName("Hello Neo"); // this works, but that's b/c Employee is the old assembly version?

            return;                  
        }


        private static void TestMethodBodyReplacementByInstance()
        {
            var employee = new Employee();

            // All LinFu.AOP-modified objects implement the IModifiableType interface
            var modifiableType = employee as IModifiableType;

            // Plug in our custom implementation
            if (modifiableType != null)
                modifiableType.MethodBodyReplacementProvider = new SimpleMethodReplacementProvider(new SampleInterceptor());

            // The employee object will call the interceptor instead of the actual method implementation
            employee.Pay(12345);
            Employee.SayName("Hello Neo");
            return;
        }

        private static void TestMethodBodyReplacementRegistry()
        {
            var provider = new SpeakProvider();
            MethodBodyReplacementProviderRegistry.SetProvider(provider); 
            
            var employee = new Employee();                       
            employee.Pay(12345);
            Employee.SayName("Hello Neo");  // crashes unexpectedly 
        }


        private static object CreateInstance(Type targetType)
        {
            var constructor = targetType.GetConstructor(new Type[0]);
            if (constructor == null)
                throw new  ApplicationException("Unable to find the default constructor");

            var instance = constructor.Invoke(new object[0]);

            return instance;
        }
    }
}
