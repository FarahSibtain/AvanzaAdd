using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class RoslynCodeRunner : SingletonBehaviour<RoslynCodeRunner>
{
    [SerializeField]
    TMPro.TMP_Text logger;

    [SerializeField]
    TMPro.TMP_InputField codeString;

    [SerializeField]
    private string[] namespaces;
    
    private string code;    

    [SerializeField]
    private UnityEvent OnRunCodeCompleted;        

    public void RunCode(string updateCode = null)
    {
        logger.text = "Executing code...";
        updateCode = string.IsNullOrEmpty(updateCode) ? null : updateCode;        

        if (string.IsNullOrEmpty(codeString.text))
        {
            logger.text = "No script provided";
            return;
        }

        try
        {           
            code = $"{(updateCode ?? codeString.text)}";
            ScriptState<object> result = CSharpScript.RunAsync(code, SetDefaultImports()).Result;
            logger.text = "Script executed successfully";
        }
        catch (Exception e)
        {
            logger.text = "Exception occurred when loading script: " + e.Message;
        }
    }

    private ScriptOptions SetDefaultImports()
    {
        return ScriptOptions.Default
            .WithImports(namespaces.Select(n => n.Replace("using", string.Empty)
           .Trim()))
            .AddReferences(
            typeof(MonoBehaviour).Assembly,
            typeof(Debug).Assembly);
    }

}
