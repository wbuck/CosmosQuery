/*
MIT License

Copyright (c) 2019 AutoMapper

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
using System.Reflection;

namespace CosmosQuery
{
    public static class CustomMethodCache
    {
        private static readonly Dictionary<string, MethodInfo> customMethods = new Dictionary<string, MethodInfo>();

        public static void CacheCustomMethod(string edmFunctionName, MethodInfo methodInfo)
            => customMethods.Add
            (
                GetMethodKey
                (
                    edmFunctionName,
                    GetArguments(methodInfo)
                ),
                methodInfo
            );

        public static MethodInfo GetCachedCustomMethod(string edmFunctionName, IEnumerable<Type> argumentTypes)
        {
            if (customMethods.TryGetValue(GetMethodKey(edmFunctionName, argumentTypes), out MethodInfo methodInfo))
                return methodInfo;

            return null;
        }

        public static bool RemoveCachedCustomMethod(string edmFunctionName, MethodInfo methodInfo)
        {
            return Remove(GetMethodKey(edmFunctionName, GetArguments(methodInfo)));

            static bool Remove(string key)
            {
                if (customMethods.ContainsKey(key))
                {
                    customMethods.Remove(key);
                    return true;
                }

                return false;
            }
        }

        private static string GetMethodKey(string edmFunctionName, IEnumerable<Type> argumentTypes)
            => string.Concat
            (
                edmFunctionName,
                ":",
                string.Join
                (
                    ",",
                    argumentTypes.Select(type => type.FullName)
                )
            );

        private static IEnumerable<Type> GetArguments(MethodInfo methodInfo)
                => methodInfo.IsStatic
                    ? methodInfo.GetParameters().Select(p => p.ParameterType)
                    : new Type[] { methodInfo.DeclaringType }
                        .Concat(methodInfo.GetParameters()
                        .Select(p => p.ParameterType));
    }
}
