using OpenerCreator.Actions;
using OpenerCreator.Helpers;
using OpenerCreator.Managers;

namespace OpenerCreatorTests;

public class ActionsMock : IActionManager
{
    public const int OldAction1 = 3640;

    private readonly IEnumerable<int> oldActions = [OldAction1];

    public string GetActionName(int action)
    {
        return oldActions.Contains(action) ? IActionManager.OldActionName : action.ToString();
    }


    public bool SameActionsByName(string action1, int action2)
    {
        return GetActionName(action2).Contains(action1, StringComparison.CurrentCultureIgnoreCase);
    }
}

public class OpenerManagerTests
{
    [Fact]
    public void Compare_WhenOpenerExecutedPerfectly_ShouldAddSuccessMessage()
    {
        // Arrange
        var openerManager = new OpenerManager(new ActionsMock())
        {
            Loaded = [1, 2, 3, 1, 2]
        };
        var used = new List<int> { 1, 2, 3, 1, 2 };
        var feedback = new Feedback();
        uint errors = 0;

        // Act
        openerManager.Compare(used, f => { feedback = f; }, _ => { errors++; });

        // Assert
        var successMessages = feedback.GetList().Where(m => m.Item1 == Feedback.MessageType.Success);
        Assert.Single(successMessages);
        Assert.Single(feedback.GetList());
        Assert.Equal(0, (int)errors);
    }

    [Fact]
    public void Compare_WhenOpenerExecutedPerfectlyWithCatchAll_ShouldAddSuccessMessage()
    {
        // Arrange
        var openerManager = new OpenerManager(new ActionsMock())
        {
            Loaded = [1, 2, 0, 1, 2]
        };
        var used = new List<int> { 1, 2, 3, 1, 2 };
        var feedback = new Feedback();
        uint errors = 0;

        // Act
        openerManager.Compare(used, f => { feedback = f; }, _ => { errors++; });

        // Assert
        var successMessages = feedback.GetList().Where(m => m.Item1 == Feedback.MessageType.Success);
        Assert.Single(successMessages);
        Assert.Single(feedback.GetList());
        Assert.Equal(0, (int)errors);
    }

    [Fact]
    public void Compare_WhenOpenerHasDifference_ShouldAddErrorMessageAndInvokeWrongAction()
    {
        // Arrange
        var openerManager = new OpenerManager(new ActionsMock())
        {
            Loaded = [1, 2, 3, 0, 2]
        };
        var used = new List<int> { 1, 5, 3, 1, 1 };
        var feedback = new Feedback();
        uint errors = 0;

        // Act
        openerManager.Compare(used, f => { feedback = f; }, _ => { errors++; });

        // Assert
        var errorMessages = feedback.GetList().Where(m => m.Item1 == Feedback.MessageType.Error);
        Assert.Equal(2, errorMessages.Count());
        Assert.Equal(2, feedback.GetList().Count);
        Assert.Equal(2, (int)errors);
    }

    [Fact]
    public void Compare_WhenOpenerShifted_ShouldAddInfoMessage()
    {
        // Arrange
        var openerManager = new OpenerManager(new ActionsMock())
        {
            Loaded = [1, 2, 3, 0, 5, 6]
        };
        var used = new List<int> { 1, 3, 4, 5, 6, 99 };
        var feedback = new Feedback();
        uint errors = 0;

        // Act
        openerManager.Compare(used, f => { feedback = f; }, _ => { errors++; });

        // Assert
        var shiftMessages = feedback.GetList().Where(m => m.Item1 == Feedback.MessageType.Info);
        var errorMessages = feedback.GetList().Where(m => m.Item1 == Feedback.MessageType.Error);
        Assert.Single(errorMessages);
        Assert.Single(shiftMessages);
        Assert.Equal(2, feedback.GetList().Count);
        Assert.Contains("by 1 action", string.Join("\n", feedback.GetMessages()));
        Assert.Equal(1, (int)errors);
    }

    [Fact]
    public void Compare_WithOldAction_ShouldFail()
    {
        // Arrange
        var openerManager = new OpenerManager(new ActionsMock())
        {
            Loaded = [2, ActionsMock.OldAction1]
        };
        var used = new List<int> { 2, 1 };
        var feedback = new Feedback();
        uint errors = 0;

        // Act
        openerManager.Compare(used, f => { feedback = f; }, _ => { errors++; });

        // Assert
        var errorMessages = feedback.GetList()
                                    .Where(m => m.Item1 == Feedback.MessageType.Error);
        Assert.Single(feedback.GetList());
        Assert.Single(errorMessages);
        Assert.Equal(1, (int)errors);
        Assert.Contains("in action 2", string.Join("\n", feedback.GetMessages()));
    }

    [Fact]
    public void Compare_WithGroupAction_ShouldSucceed()
    {
        // Arrange
        var openerManager = new OpenerManager(new ActionsMock())
        {
            Loaded = [2, -1, 1, -1, -1]
        };
        var used = new List<int> { 2, 15999, 1, 16000, 15999 };
        var feedback = new Feedback();
        uint errors = 0;

        // Act
        openerManager.Compare(used, f => { feedback = f; }, _ => { errors++; });

        // Assert
        var errorMessages = feedback.GetList()
                                    .Where(m => m.Item1 == Feedback.MessageType.Error);
        Assert.Single(feedback.GetList());
        Assert.Empty(errorMessages);
        Assert.Equal(0, (int)errors);
    }

    [Fact]
    public void Compare_WithGroupActionAndWrongAction_ShouldFail()
    {
        // Arrange
        var openerManager = new OpenerManager(new ActionsMock())
        {
            Loaded = [2, -1, 1, -1, -1]
        };
        var used = new List<int> { 2, 15999, 1, 1, 15999 };
        var feedback = new Feedback();
        uint errors = 0;

        // Act
        openerManager.Compare(used, f => { feedback = f; }, _ => { errors++; });

        // Assert
        var errorMessages = feedback.GetList()
                                    .Where(m => m.Item1 == Feedback.MessageType.Error);
        Assert.Single(feedback.GetList());
        Assert.Single(errorMessages);
        Assert.Equal(1, (int)errors);
        Assert.Contains("in action 4", string.Join("\n", feedback.GetMessages()));
    }
}
