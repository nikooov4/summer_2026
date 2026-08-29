module Mephi.Cybernetics.Race.HostEnvironments.Tests.ObjTemplateTest

open Mephi.Cybernetics.Race.Core.Host.Definitions
open Mephi.Cybernetics.Race.Core.Host.Definitions.Tff
open Mephi.Cybernetics.Race.Core.Host.Computing

let template = DefaultMultiLambda([|DefaultVariable("XXX")|],DefaultVariable("XXX"))

// Protocol = (student,(avgMark,count3,count4,count5),(presentationDuration,pageCount,mark))