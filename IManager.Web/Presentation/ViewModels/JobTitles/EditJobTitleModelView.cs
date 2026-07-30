using System.ComponentModel.DataAnnotations;

namespace IManager.Web.Presentation.ViewModels.JobTitles;

public class EditJobTitleModelView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsHazard { get; set; } = false;
    public bool IsUnhealthy { get; set; } = false;
    public bool IsCommissioned { get; set; } = false;
    public bool IsTimeBank { get; set; } = false;

    [DisplayFormat(DataFormatString = @"{0:hh\:mm\:ss}", ApplyFormatInEditMode = true)]
    public TimeSpan DailyHours { get; set; } = TimeSpan.FromHours(8);
}
