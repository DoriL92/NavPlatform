using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Common.Options;
public sealed class PublicLinkOptions
{
    public string BaseUrl { get; set; } = "http://localhost:7002/public";
}