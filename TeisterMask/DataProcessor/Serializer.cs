namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            //var sb = new StringBuilder();
            //var xmlSerializer = new XmlSerializer(typeof(List<ProjectsDto>),
            //    new XmlRootAttribute("Projects"));

            //var projects = context.Projects
            //    .Where(p => p.Tasks.Any())
            //    .Select(p => new ProjectsDto
            //    {
            //        Name = p.Name,
            //        TaskCount = p.Tasks.Count,
            //        DueDate = GetEndDate(p.DueDate),
            //        Tasks = p.Tasks.Select(t => new TaskDto
            //        {
            //            Name = t.Name,
            //            LabelType = t.LabelType.ToString()
            //        })
            //        .OrderBy(t => t.Name)
            //        .ToArray()
            //    })
            //    .OrderByDescending(p => p.TaskCount)
            //    .ThenBy(p => p.Name)
            //    .ToList();

            //var namespaces = new XmlSerializerNamespaces();
            //namespaces.Add("", "");

            //using (var writer = new StringWriter(sb))
            //{
            //    xmlSerializer.Serialize(writer, projects, namespaces);
            //}

            //return sb.ToString().TrimEnd();

            return "";
        }

        private static string GetEndDate(DateTime? dueDate)
        {
            var hasEndDate = string.Empty;
            if (dueDate == null)
            {
                hasEndDate = "No";
            }
            else
            {
                hasEndDate = "Yes";
            }
            return hasEndDate;
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context.Employees
                .Where(e => e.EmployeesTasks.Any(et => et.Task.OpenDate >= date))
                .Select(e => new
                {
                    Username = e.Username,
                    Tasks = e.EmployeesTasks.Select(et => new
                    {
                        TaskName = et.Task.Name,
                        OpenDate = et.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                        DueDate = et.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                        LabelType = et.Task.LabelType.ToString(),
                        ExecutionType = et.Task.EmployeesTasks.ToString()
                    })
                    .OrderByDescending(et => et.DueDate)
                    .ThenBy(et => et.TaskName)
                    .ToList()
                })
                .OrderByDescending(e => e.Tasks.Count)
                .ThenBy(e => e.Username)
                .Take(10)
                .ToList();

            var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
        }
    }
}