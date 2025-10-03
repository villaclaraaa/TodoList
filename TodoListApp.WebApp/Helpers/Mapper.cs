using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Helpers;

public static class Mapper
{
    public static TodoList MapApiModelToDomain(TodoListWebApiModel apiModel)
    {
        if (apiModel == null)
        {
            return null;
        }

        return new TodoList
        {
            Id = apiModel.Id,
            Title = apiModel.Title,
            Description = apiModel.Description,
            OwnerId = apiModel.OwnerId,
            Tasks = apiModel.Tasks?.Select(t => MapApiModelToDomain(t)).ToList() ?? new List<TodoTask>()
        };
    }

    public static TodoListWebApiModel MapDomainToApiModel(TodoList domainModel)
    {
        if (domainModel == null)
        {
            return null;
        }

        return new TodoListWebApiModel
        {
            Id = domainModel.Id,
            Title = domainModel.Title,
            Description = domainModel.Description,
            OwnerId = domainModel.OwnerId,
            Tasks = domainModel.Tasks?.Select(t => MapDomainToApiModel(t)).ToList() ?? new List<TodoTaskWebApiModel>()
        };
    }

    public static TodoTask MapApiModelToDomain(TodoTaskWebApiModel apiModel)
    {
        if (apiModel == null)
        {
            return null;
        }

        return new TodoTask
        {
            Id = apiModel.Id,
            Title = apiModel.Title,
            Description = apiModel.Description,
            TodoListId = apiModel.TodoListId,
            Status = (Models.TaskStatus)apiModel.Status,
            DueDate = apiModel.DueDate,
            CreatedAt = apiModel.CreatedAt,
            AssignedUserId = apiModel.AssignedUserId,
            Tags = apiModel.Tags?.ToList() ?? new List<string>(),
            Comments = apiModel.Comments?.ToList() ?? new List<string>()
        };
    }

    public static TodoTaskWebApiModel MapDomainToApiModel(TodoTask domainModel)
    {
        if (domainModel == null)
        {
            return null;
        }

        return new TodoTaskWebApiModel
        {
            Id = domainModel.Id,
            Title = domainModel.Title,
            Description = domainModel.Description,
            TodoListId = domainModel.TodoListId,
            Status = (Models.TaskStatus)domainModel.Status,
            DueDate = domainModel.DueDate,
            CreatedAt = domainModel.CreatedAt,
            AssignedUserId = domainModel.AssignedUserId,
            Tags = domainModel.Tags?.ToList() ?? new List<string>(),
            Comments = domainModel.Comments?.ToList() ?? new List<string>()
        };
    }

    public static TodoListModel MapDomainToViewModel(TodoList domainModel)
    {
        if (domainModel == null)
        {
            return null;
        }

        return new TodoListModel
        {
            Id = domainModel.Id,
            Title = domainModel.Title,
            Description = domainModel.Description,
            OwnerId = domainModel.OwnerId,
            Tasks = domainModel.Tasks?.Select(t => new TodoTaskModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                TodoListId = t.TodoListId,
                Status = (Models.TaskStatus)t.Status,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                AssignedUserId = t.AssignedUserId,
                Tags = t.Tags?.ToList() ?? new List<string>(),
                Comments = t.Comments?.ToList() ?? new List<string>()
            }).ToList() ?? new List<TodoTaskModel>()
        };
    }

    public static TodoList MapViewModelToDomain(TodoListModel viewModel)
    {
        if (viewModel == null)
        {
            return null;
        }

        return new TodoList
        {
            Id = viewModel.Id,
            Title = viewModel.Title,
            Description = viewModel.Description,
            OwnerId = viewModel.OwnerId,
            Tasks = viewModel.Tasks?.Select(t => new TodoTask
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                TodoListId = t.TodoListId,
                Status = (Models.TaskStatus)t.Status,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                AssignedUserId = t.AssignedUserId,
                Tags = t.Tags?.ToList() ?? new List<string>(),
                Comments = t.Comments?.ToList() ?? new List<string>()
            }).ToList() ?? new List<TodoTask>()
        };
    }

    public static TodoTask MapTaskViewModelToDomain(TodoTaskModel viewModel)
    {
        if (viewModel == null)
        {
            return null;
        }

        return new TodoTask
        {
            Id = viewModel.Id,
            Title = viewModel.Title,
            Description = viewModel.Description,
            TodoListId = viewModel.TodoListId,
            Status = (Models.TaskStatus)viewModel.Status,
            DueDate = viewModel.DueDate,
            CreatedAt = viewModel.CreatedAt,
            AssignedUserId = viewModel.AssignedUserId,
            Tags = (List<string>)viewModel.Tags,
            Comments = (List<string>)viewModel.Comments
        };
    }

    public static TodoTaskModel MapTaskDomainToViewModel(TodoTask domainModel)
    {
        if (domainModel == null)
        {
            return null;
        }

        return new TodoTaskModel
        {
            Id = domainModel.Id,
            Title = domainModel.Title,
            Description = domainModel.Description,
            TodoListId = domainModel.TodoListId,
            Status = (Models.TaskStatus)domainModel.Status,
            DueDate = domainModel.DueDate,
            CreatedAt = domainModel.CreatedAt,
            AssignedUserId = domainModel.AssignedUserId,
            Tags = domainModel.Tags,
            Comments = domainModel.Comments
        };
    }
}
