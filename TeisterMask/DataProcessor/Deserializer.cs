namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    using Data;
    using System.Xml.Serialization;
    using TeisterMask.DataProcessor.ImportDto;
    using System.IO;
    using System.Text;
    using TeisterMask.Data.Models;
    using System.Linq;
    using System.Globalization;
    using TeisterMask.Data.Models.Enums;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ProjectsDto>),
                new XmlRootAttribute("Projects"));

            var projectDtos = new List<ProjectsDto>();

            using (var reader = new StringReader(xmlString))
            {
                projectDtos = (List<ProjectsDto>)xmlSerializer.Deserialize(reader);
            }

            var sb = new StringBuilder();
            var projects = new List<Project>();

            foreach (var projectsDto in projectDtos)
            {
                if (IsValid(projectsDto))
                {
                    var tasks = new List<Task>();

                    foreach (var task in projectsDto.Tasks)
                    {
                        var execType = Enum.Parse<ExecutionType>(task.ExecutionType);
                        var labelType = Enum.Parse<LabelType>(task.LabelType);

                        if (execType == null || labelType == null)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (IsValid(task))
                        {
                            var currentTask = new Task
                            {
                                Name = task.Name,
                                OpenDate = DateTime.ParseExact(task.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                DueDate = DateTime.ParseExact(task.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                ExecutionType = Enum.Parse<ExecutionType>(task.ExecutionType),
                                LabelType = Enum.Parse<LabelType>(task.LabelType)
                            };

                            if (string.IsNullOrWhiteSpace(projectsDto.DueDate))
                            {
                                var isValidDate = DateTime.ParseExact(projectsDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                    < DateTime.ParseExact(task.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                                if (isValidDate)
                                {
                                    tasks.Add(currentTask);
                                }
                                else
                                {
                                    context.Tasks.Add(currentTask);
                                    sb.AppendLine(ErrorMessage);
                                }
                            }
                            else if (DateTime.ParseExact(projectsDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                    < DateTime.ParseExact(task.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                    && DateTime.ParseExact(projectsDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                    > DateTime.ParseExact(task.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                            {
                                tasks.Add(currentTask);
                            }
                            else
                            {
                                sb.AppendLine(ErrorMessage);
                            }
                        }
                        else
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }
                    }
                    var project = new Project
                    {
                        Name = projectsDto.Name,
                        OpenDate = DateTime.ParseExact(projectsDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        DueDate = SetDate(projectsDto.DueDate),
                        Tasks = tasks
                    };

                    projects.Add(project);
                    sb.AppendLine($"Successfully imported project - {project.Name} with {project.Tasks.Count} tasks.");
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
            }

            //context.Projects.AddRange(projects);
            //context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        private static DateTime? SetDate(string dueDate)
        {
            if (string.IsNullOrWhiteSpace(dueDate))
            {
                return new DateTime();
            }
            return DateTime.ParseExact(dueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var employeeDtos = JsonConvert.DeserializeObject<ImportEmplyeesDto[]>(jsonString);

            var emplyees = new List<Employee>();

            foreach (var dto in employeeDtos)
            {
                var employeeTasks = new List<EmployeeTask>();

                if (IsValid(dto) && dto.Tasks.Any())
                {
                    var emplyee = new Employee
                    {
                        Username = dto.Username,
                        Email = dto.Email,
                        Phone = dto.Phone
                    };

                    foreach (var task in dto.Tasks)
                    {
                        var tasks = new List<Task>();
                        var currentTask = context.Tasks.FirstOrDefault(t => t.Id == task);

                        if (currentTask == null)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (!tasks.Contains(currentTask))
                        {
                            tasks.Add(currentTask);

                            var employeeTask = new EmployeeTask
                            {
                                Employee = emplyee,
                                Task = currentTask
                            };

                            employeeTasks.Add(employeeTask);
                        }
                    }

                    emplyee.EmployeesTasks = employeeTasks;

                    emplyees.Add(emplyee);
                    sb.AppendLine($"Successfully imported employee - {emplyee.Username} with {emplyee.EmployeesTasks.Count} tasks.");
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
            }

            context.Employees.AddRange(emplyees);
            //context.SaveChanges();

            return sb.ToString();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}