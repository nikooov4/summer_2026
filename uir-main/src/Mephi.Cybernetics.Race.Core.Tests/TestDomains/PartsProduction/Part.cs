using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.Core.UI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mephi.Cybernetics.Race.Core.Tests.TestDomains.PartsProduction;

[DisplayName("Деталь")]
[IsEntity]
public class Part
{
    [DisplayName("Серийный номер детали")]
    [Description("Серийный номер, наштампованный на детали заводом")]
    [PropertyGroup("Номенклатурные сведения")]
    public Guid PartNumber { get; }

    [IsOptional]
    public string Name { get; }

    [IsOptional(false)]
    [DefaultValue(0)]
    public float Weight { get; }
}

[IsEntity]
public class Product
{
    [IdField(1)]
    public Guid Serial { get; }
    [IdField(2)]
    public string Name { get; }
    [IsComputable(nameof(Parts))]
    public float Weight => Parts.Sum(p => p.Weight);
    public IList<Part> Parts { get; }
}

