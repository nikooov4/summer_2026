using System;
using Irony;
using Irony.Parsing;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Xml.Linq;
using System.Reflection.Metadata;
using System.Diagnostics;
using System.Net.Sockets;


namespace Mephi.Cybernetics.Race.MappingDescriptionLanguage
{
    public class Ellipse2 
    {
        public Point Location { get; set; }
        public Size Size { get; set; }
    }
    public class Ellipse 
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int LongSemiAxis { get; set; }
        public int ShortSemiAxis { get; set; }
    }

    public class Ellipse3
    {
        public List<Point> Points {  get; set; }
    }

    public class Ellipse4
    {
        public List<List<int>> ListOfList { get; set; }
        public List<int> IntList { get; set; }
        public int Count { get; set; }

    }


    public class CustomLanguageCompiler
    {
        public CustomLanguageCompiler()
        {
        }

        public Action<S, D> Compile<S,D>(string code)
        {
            var grammar = new MappingDSLGrammar();
            var parser = new Parser(grammar);
            var parseTree = parser.Parse(code);

            var astBuilder = new AstBuilder();
            var ast = astBuilder.BuildAst<S,D>(parseTree.Root, code);
            var res = ast.Compile();
            return res;
        }
    }

    internal static class AstExtensions
    {
        public static bool TryEvaluate<S,D>(this ParseTreeNode node, out Action<S,D> result)
        {
            result = null;

            if (node == null || node.Term == null)
                return false;

            switch (node.Term.Name)
            {
                case TokenConsts.assignment:
                    {
                        if (node.ChildNodes[1].Term.Name == TokenConsts.objectCreation)
                        {
                            var destinationNode = node.ChildNodes[0];
                            var objectCreationNode = node.ChildNodes[1];

                            result = ((input, output) =>
                            {
                                var (destinationProperty, destinationValue) = EvaluatePropertyAccess(destinationNode, output);
                                var objectValue = CreateObject(objectCreationNode, destinationProperty.PropertyType, input);

                                // Set the property value of @out
                                SetPropertyValue(output, destinationProperty, objectValue);
                            });   
                        } else if (node.ChildNodes[1].Term.Name == TokenConsts.functionCall)
                        {
                            if (node.ChildNodes[1].ChildNodes[0].Term.Name == TokenConsts.selectFunction)
                            {
                                var destinationNode = node.ChildNodes[0];
                                var selectNode = node.ChildNodes[1].ChildNodes[0];

                                result = (input, output) =>
                                {
                                    var localVariables = new Dictionary<string, object>() { { TokenConsts.sourceKeyword, input }, { TokenConsts.destinationKeyword, output } };
                                    var localVariableTypes = new Dictionary<string, Type>() { { TokenConsts.sourceKeyword, input.GetType() }, { TokenConsts.destinationKeyword, output.GetType() } };

                                    var map = CreateSelectAssignmentAction(selectNode, localVariables, localVariableTypes);

                                    var lambdaArgNode = selectNode.ChildNodes[1];
                                    var (_, inputList) = EvaluatePropertyAccess(lambdaArgNode, input);
                                    var destVal = map(inputList);
                                    var (destinationProperty, _) = EvaluatePropertyAccess(destinationNode, output);
                                    destinationProperty.SetValue(output, destVal);
                                };
                            }
                        }
                        else
                        {
                            var destinationNode = node.ChildNodes[0]; 
                            var sourceNode = node.ChildNodes[1]; 

                            result = ((input, output) =>
                            {
                                var (_, sourceValue) = EvaluatePropertyAccess(sourceNode, input);
                                var (destinationProperty, _) = EvaluatePropertyAccess(destinationNode, output);

                                SetPropertyValue(output, destinationProperty, sourceValue);
                            });
                        }
                        return true;

                    }
                default:
                    return false;
            }
        }

        private static (PropertyInfo, Type type) CalcNestedPropertyAccessType(ParseTreeNode node, Type type)
        {
            var propertyName = node.ChildNodes[0].Token.Text;
            var property = type.GetProperty(propertyName);
            var outType = property.PropertyType;
            if (node.ChildNodes.Count == 2)
            {
                (property, outType) = CalcNestedPropertyAccessType(node.ChildNodes[1], outType);
            }
            return (property, outType);
        }


        private static (PropertyInfo, Type type) CalcPropertyAccessType(ParseTreeNode node, Type type)
        {
            PropertyInfo property = null;
            Type outType;

            if (node.Term.Name == TokenConsts.generalPropertyAccess)
            {
                if (node.ChildNodes[0].ChildNodes.Count == 1)
                {
                    outType = type;
                }
                else
                    (property, outType) = CalcNestedPropertyAccessType(node.ChildNodes[0].ChildNodes[1], type);
            }
            else if (node.Term.Name == TokenConsts.sourcePropertyAccess || node.Term.Name == TokenConsts.destPropertyAccess)
            {
                if (node.ChildNodes.Count == 1)
                    outType = type;
                else
                    (property, outType) = CalcNestedPropertyAccessType(node.ChildNodes[1], type);
            } else
            {
                throw new NotImplementedException();
            }
            
            return (property, outType);
        }

        private static (PropertyInfo, object propertyValue) EvaluateNestedPropertyAccess(ParseTreeNode node, object obj)
        {
            var propertyName = node.ChildNodes[0].Token.Text;
            var property = obj.GetType().GetProperty(propertyName);
            var value = property.GetValue(obj);
            if (node.ChildNodes.Count == 2)
            {
                (property, value) = EvaluateNestedPropertyAccess(node.ChildNodes[1], value);
            }
            return (property, value);
        }


        private static (PropertyInfo, object propertyValue) EvaluatePropertyAccess(ParseTreeNode node, object obj)
        {
            PropertyInfo property = null;
            object value = null;

            if (node.Term.Name == TokenConsts.generalPropertyAccess)
            {
                if (node.ChildNodes[0].ChildNodes.Count == 1)
                {
                    value = obj;
                }
                else
                    (property, value) = EvaluateNestedPropertyAccess(node.ChildNodes[0].ChildNodes[1], obj);
            }
            else if (node.Term.Name == TokenConsts.sourcePropertyAccess || node.Term.Name == TokenConsts.destPropertyAccess)
            {
                if (node.ChildNodes.Count == 1)
                    value = obj;
                else
                    (property, value) = EvaluateNestedPropertyAccess(node.ChildNodes[1], obj);
            }
            else
            {
                throw new NotImplementedException();
            }

            return (property, value);
        }

        private static void SetPropertyValue(object destObj, PropertyInfo destProp, object newValue)
        {
            destProp.SetValue(destObj, newValue);
        }

        private static object CreateObject(ParseTreeNode node, Type objType, object input)
        {
            // Create an object based on key-value pairs
            var result = Activator.CreateInstance(objType); // Assuming property is a PropertyInfo object
            foreach (var keyValueNode in node.ChildNodes[0].ChildNodes)
            {
                var key = keyValueNode.ChildNodes[0].Token.Text;
                var (inputProp, inputVal) = EvaluatePropertyAccess(keyValueNode.ChildNodes[1], input);
                //var value = keyValueNode.ChildNodes[1].Token.Text;
                var property = result.GetType().GetProperty(key);
                property.SetValue(result, Convert.ChangeType(inputVal, property.PropertyType));
            }
            return result;
        }

        // DO NOT DELETE IT !!!
        private static List<T> ConvertArray<T>(IList input)
        {
            var t = typeof(T);
            return input.Cast<T>().ToList();
        }

        public static object GetDeserializedObject(IList obj, Type targetType)
        {
            MethodInfo convertMethod = typeof(AstExtensions).GetMethod("ConvertArray",
                     BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo generic = convertMethod.MakeGenericMethod(new[] { targetType });
            var res = generic.Invoke(null, new object[] { obj });
            return res;
        }

        private static Type CalcFunctionReturnType(ParseTreeNode functionCallNode, Dictionary<string, Type> localVariableTypes)
        {
            var funcNode = functionCallNode.ChildNodes[0];
            if (funcNode.Term.Name == TokenConsts.selectFunction)
            {
                return CalcSelectReturnListType(funcNode, localVariableTypes);
            }

            throw new NotImplementedException();

        }

        private static Type CalcLambdaReturnType(ParseTreeNode lambdaExpressionNode, Dictionary<string, Type> localVariableTypes)
        {
            var inputTypeName = lambdaExpressionNode.ChildNodes[0].Token.Text;
            var inputType = localVariableTypes[inputTypeName];
            var lambdaBody = lambdaExpressionNode.ChildNodes[^1].ChildNodes[0];
            Type returnType = null;
            if (lambdaBody.Term.Name == TokenConsts.generalPropertyAccess)
            {
                returnType = CalcPropertyAccessType(lambdaBody, inputType).type;
            } else if (lambdaBody.Term.Name == TokenConsts.functionCall)
            {
                returnType = CalcFunctionReturnType(lambdaBody, localVariableTypes);
            }
            else
            {
                throw new NotImplementedException();
            }
            localVariableTypes.Remove(inputTypeName);
            return returnType;
        }

        private static Type CalcSelectReturnListType(ParseTreeNode selectFunctionNode, Dictionary<string, Type> localVariableTypes)
        {
            var listArgNode = selectFunctionNode.ChildNodes[1];
            var inputObjType = localVariableTypes[listArgNode.ChildNodes[0].ChildNodes[0].Token.Text];
            var (_, inputList) = CalcPropertyAccessType(listArgNode, inputObjType);
            var inputListGenericType = inputList.GetGenericArguments().First();

            var lambdaExpressionNode = selectFunctionNode.ChildNodes[^1];
            var lambdaInputVarName = lambdaExpressionNode.ChildNodes[0].Token.Text;
            localVariableTypes.Add(lambdaInputVarName, inputListGenericType);
            var genericListType = CalcLambdaReturnType(lambdaExpressionNode, localVariableTypes);
            return typeof(List<>).MakeGenericType(genericListType);
        }


        private static Func<object, object> CreateSelectAssignmentAction(ParseTreeNode selectFunctionNode, Dictionary<string, object> localVariables, Dictionary<string, Type> localVariableTypes)
        {
            var outListType = CalcSelectReturnListType(selectFunctionNode, localVariableTypes);
            var genericListType = outListType.GetGenericArguments().First();

            return (input) =>
            {
                Debug.Assert(input is IList);
                //var (destinationProperty, destinationValue) = EvaluatePropertyAccess(destinationNode, output);


                //var lambdaArgNode = selectFunctionNode.ChildNodes[1].ChildNodes[0];
                //var inputObjName = lambdaArgNode.ChildNodes[0].Token.Text;
                //var inputObj = localVariables[inputObjName];
                //var (_, inputList) = EvaluatePropertyAccess(lambdaArgNode, inputObj);


            

                var lambdaExpressionNode = selectFunctionNode.ChildNodes[^1];
                var lambda = CreateLambdaFunction(lambdaExpressionNode, localVariables, localVariableTypes, input.GetType());

                var selectedValues = LanguageUtils.CreateList(genericListType);
                foreach (var item in input as IList)
                {
                    selectedValues.Add(lambda(item));
                }

                return GetDeserializedObject(selectedValues, genericListType);

                //List<object> lo = new List<object>((IEnumerable)ll);

                //((IEnumerable)(IList)ll).ToList();
            };
        }

        private static Func<object, object> CreateLambdaFunction(ParseTreeNode lambdaExpressionNode, Dictionary<string, object> localVariables, Dictionary<string, Type> localVariableTypes, Type inputType)
        {
            var lambdaBody = lambdaExpressionNode.ChildNodes[^1].ChildNodes[^1];
            var inputListGenericType = inputType.GetGenericArguments().First();
            localVariableTypes.TryAdd(lambdaExpressionNode.ChildNodes[0].Token.Text, inputListGenericType);

            if (lambdaBody.Term.Name == TokenConsts.functionCall)
            {
                var functionCall = lambdaBody.ChildNodes[0];
                if (functionCall.Term.Name == TokenConsts.selectFunction)
                {
                    var selectNode = lambdaBody.ChildNodes[0];

                    var map = CreateSelectAssignmentAction(selectNode, localVariables, localVariableTypes);

                    return (inputObj) => {
                        var lambdaArgNode = selectNode.ChildNodes[1];
                        var (_, inputList) = EvaluatePropertyAccess(lambdaArgNode, inputObj);
                        return map(inputList);
                    };
                }
                throw new NotImplementedException();
            }
            return (object input) => {
                var (property, value)= EvaluatePropertyAccess(lambdaBody, input);
                return value;
            };
        }

    }
}