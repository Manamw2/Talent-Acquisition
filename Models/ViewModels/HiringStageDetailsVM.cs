using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class HiringStageDetailsVM
    {
        public HiringStage HiringStage { get; set; }
        public List<OutcomeVm> Outcomes { get; set; } = new List<OutcomeVm>();
        public List<ParameterVm> Parameters { get; set; } = new List<ParameterVm>();
    }
    public class OutcomeVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NotificationMessage { get; set; }
        public string ApplicationStatus { get; set; }
    }
    public class ParameterVm()
    {
        public string Name { get; set; }
    }
}