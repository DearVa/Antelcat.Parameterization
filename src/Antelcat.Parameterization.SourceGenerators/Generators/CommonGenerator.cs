﻿using Microsoft.CodeAnalysis;

namespace Antelcat.Parameterization.SourceGenerators.Generators;

public static class CommonGenerator
{
    public static void Execute(in SourceProductionContext context)
    {
        context.AddSource($"{Global.Namespace}.Common.g.cs",
            $$""""
              // <auto-generated/>
              #nullable enable
              using System;
              using System.Collections.Generic;
              using System.Text.RegularExpressions;

              namespace Antelcat.Parameterization
              {
                  public class CommandNotFoundException(string commandName) : Exception($"Command \"{commandName}\" not found.") { }
                  
                  public class ParsedArgument
                  {
                      public string Name { get; }
                      
                      public virtual object? Value => value;
                      public bool HasValue => hasValue;
                      
                      protected object? value;
                      protected bool hasValue;
                      
                      public ParsedArgument(string name)
                      {
                          Name = name;
                      }
                      
                      public virtual void SetValue(object? value)
                      {
                          if (hasValue)
                          {
                              throw new ArgumentException($"Multiple value of argument \"{Name}\" is not supported.");
                          }
                          
                          this.value = value;
                          hasValue = true;
                      }
                  }
                  
                  public class ParsedArrayArgument<T> : ParsedArgument
                  {
                      public ParsedArrayArgument(string name) : base(name)
                      {
                          value = new List<T>(1);
                      }
                      
                      public override object? Value => ((List<T>)value!).ToArray();
                      
                      public override void SetValue(object? value)
                      {
                          ((List<T>)this.value!).Add(((T)value!)!);
                          hasValue = true;
                      }
                  }
              
                  public static class Common
                  {
                      public static Regex CommandRegex { get; } = new Regex(@"[^\s""]+|""([^""]|(\\""))*""");
                      public static Regex ArgumentRegex { get; } = new Regex(@"[^,""]+|""([^""]|(\\""))*""");
                      public static Regex QuotationRegex { get; } = new Regex(@"^""|""$");
                  
                      internal static int FindIndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicate) {
                          for (var i = 0; i < list.Count; i++) {
                              if (predicate(list[i])) {
                                  return i;
                              }
                          }
              
                          return -1;
                      }
              
                      public static void ParseArguments(
                          IReadOnlyList<ParsedArgument> parsedArguments,
                          IReadOnlyList<string> arguments,
                          IReadOnlyList<(string fullName, string? shortName)> parameterNames,
                          IReadOnlyList<string?> defaultValues,
                          IReadOnlyList<{{Global.TypeConverter}}> argumentConverters,
                          bool ignoreCase)
                      {
                          var isNamedArgumentUsed = false;
                          for (var i = 1; i < arguments.Count; i++)
                          {
                              var argumentIndex = -1;
                              var argument = arguments[i];
                              
                              if (argument.StartsWith("---"))
                              {
                                  throw new ArgumentException($"Bad switch syntax: {argument}");
                              }
                              if (argument.StartsWith("--"))
                              {
                                  argumentIndex = parameterNames.FindIndexOf(x => argument[2..].Equals(x.fullName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                                  if (argumentIndex == -1)
                                  {
                                      throw new ArgumentException($"Argument \"{argument}\" not found.");
                                  }
                                  isNamedArgumentUsed = true;
                              }
                              else if (argument.StartsWith('-'))
                              {
                                  argumentIndex = parameterNames.FindIndexOf(x => argument[1..].Equals(x.shortName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                                  if (argumentIndex == -1)
                                  {
                                      throw new ArgumentException($"Argument \"{argument}\" not found.");
                                  }
                                  isNamedArgumentUsed = true;
                              }
                              else if (isNamedArgumentUsed)
                              {
                                  throw new ArgumentException("Named results must come after all anonymous results.");
                              }
              
                              if (argumentIndex != -1)
                              {
                                  // 当前是命名参数，那么下一个才是参数的值
                                  if (i == arguments.Count - 1 || arguments[i + 1].StartsWith('-'))
                                  {
                                      // 如果没有下一个参数，或者下一个参数是命名参数
                                      var defaultValue = defaultValues[argumentIndex];
                                      if (defaultValue == null)
                                      {
                                          throw new ArgumentException($"The value of argument \"{argument}\" is not specified.");
                                      }
                                      
                                      parsedArguments[argumentIndex].SetValue(argumentConverters[argumentIndex].ConvertFromString(defaultValue));
                                      continue;
                                  }
                                  
                                  for (++i; i < arguments.Count; i++)
                                  {
                                      argument = arguments[i];
                                      if (argument.StartsWith('-'))
                                      {
                                          i--;
                                          break;
                                      }
                                      
                                      parsedArguments[argumentIndex].SetValue(argumentConverters[argumentIndex].ConvertFromString(argument));
                                  }
                              }
                              else
                              {
                                  argumentIndex = i - 1;
                                  parsedArguments[argumentIndex].SetValue(argumentConverters[argumentIndex].ConvertFromString(argument));
                              }
                          }
                      }
                      
                      public static T ConvertArgument<T>(ParsedArgument parsed)
                      {
                          if (!parsed.HasValue) throw new ArgumentException($"Argument \"{parsed.Name}\" is not specified.");
                          return parsed.Value is T result ? result : throw new ArgumentException($"Argument \"{parsed.Name}\" is not of type {typeof(T).FullName}.");
                      }
                      
                      public static T ConvertArgument<T>(ParsedArgument parsed, T defaultValue)
                      {
                          if (!parsed.HasValue) return defaultValue;
                          return parsed.Value is T result ? result : throw new ArgumentException($"Argument \"{parsed.Name}\" is not of type {typeof(T).FullName}.");
                      }
                  }
              }
              """");
    }
}