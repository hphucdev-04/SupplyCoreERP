using System;

namespace SupplyCoreERP;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class QATestAttribute : Attribute
{
    public string Scenario { get; }
    public string Feature { get; }
    public string Layer { get; }
    public string Priority { get; }
    public string Note { get; }

    public QATestAttribute(
        string scenario,
        string feature,
        string layer,
        string priority = "Medium",
        string note = "Đã kiểm chứng thành công.")
    {
        Scenario = scenario;
        Feature = feature;
        Layer = layer;
        Priority = priority;
        Note = note;
    }
}
