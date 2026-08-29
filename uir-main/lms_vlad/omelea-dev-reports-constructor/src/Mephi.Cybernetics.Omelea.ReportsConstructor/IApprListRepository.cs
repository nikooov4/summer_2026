using Mephi.Cybernetics.Omelea.ReportsConstructor.Models;
using Mephi.Cybernetics.Race.ReportsDescriptionLanguage;
using System;
using System.Threading.Tasks;

namespace Mephi.Cybernetics.Omelea.ReportsConstructor
{
    public interface IApprListRepository : IDisposable
    {
        Task SaveDefsAsync(ConceptManager conceptManager);
        Task SaveMarksAsync(ConceptDef apprListConceptDef);
        Task<ConceptDef> LoadMarksAsync(ConceptDef apprListConceptDef);
        Task<ConceptManager> LoadApprListDefsAsync();
        Task<ConceptManager> LoadStudentsAsync(ConceptManager conceptManager);
    }
} 