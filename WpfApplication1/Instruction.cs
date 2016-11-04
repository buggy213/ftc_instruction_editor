using System;
using System.Collections.Generic;
using System.Text;

public enum InstructionParameterType
{
    DOUBLE, FLOAT, STRING, INTEGER, CHARACTER
}

public class Instruction
{
    public InstructionTemplate template;

    // Key = parameter name, value = parameter value
    public Dictionary<string, string> instructionParameters;
    public Instruction (Dictionary <string, string> instructionParameters)
    {
        this.instructionParameters = instructionParameters;
    }
    public Instruction (InstructionTemplate template)
    {
        this.template = template;
        instructionParameters = new Dictionary<string, string>();
        foreach (string key in template.parameters.Keys)
        {
            instructionParameters.Add(key, null);
        }
    }

    public void FromString ()
    {

    }

    public string PrintType ()
    {
        return "\"type\" = \"" + template.type + "\""; 
    }

    public string WriteToJson()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{ \"type\" = \"" + template.type + "\"");
        foreach (KeyValuePair <string, string> kvp in instructionParameters)
        {
            sb.Append(", \"" + kvp.Key + "\" = " + kvp.Value);
        }
        sb.Append(" }");
        sb.AppendLine();
        return sb.ToString();
    }

    public override string ToString()
    {
        return template.type;
    }

}
public class InstructionTemplate
{
    public string type;
    public Dictionary<string, InstructionParameterType> parameters;
    public InstructionTemplate ()
    {

    }
    public InstructionTemplate (Dictionary<string, InstructionParameterType> parameters)
    {
        this.parameters = parameters;
    }

    // Checks to see if the parameter is actually valid (i.e 5.0 isn't an integer)
    public static bool CheckValidParameter (InstructionParameterType parameterType, string input)
    {
        try
        {
            switch (parameterType)
            {
                case InstructionParameterType.DOUBLE:
                    double.Parse(input);
                    break;

                case InstructionParameterType.FLOAT:
                    float.Parse(input);
                    break;

                case InstructionParameterType.CHARACTER:
                    char.Parse(input);
                    break;

                case InstructionParameterType.INTEGER:
                    char.Parse(input);
                    break;

                case InstructionParameterType.STRING:
                    break;
            }
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }
}