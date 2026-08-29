using System;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Mephi.Cybernetics.Race.Core;

namespace Mephi.Cybernetics.Omelea.Services.Web.Bootstrap;

public class ControllerBuilder
{
    public static Controller Build<T>()
    {
        var typeDescription = ObjectDescription.Get<T>();
        if (!typeDescription.ClrAttributes.HasAttribute<ServiceContractAttribute>())
            throw new ArgumentException("The 'T' type must be a ServiceContract");
        throw new NotImplementedException();
    }
}
