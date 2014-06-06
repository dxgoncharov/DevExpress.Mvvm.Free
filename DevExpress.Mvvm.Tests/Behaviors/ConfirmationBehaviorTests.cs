#if !SILVERLIGHT
using NUnit.Framework;
#else
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class ConfirmationBehaviorTests {
        public class TestViewModelBase : ViewModelBase {
            public IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        }
        public class TestControl1 : Control {
            public ICommand Command { get; set; }
        }
        public class TestMessageBoxService : IMessageBoxService {
            public string MessageBoxTest;
            public string Caption;
            public MessageBoxButton Button;
            public MessageBoxResult Result = MessageBoxResult.Yes;
            public MessageBoxResult DefaultResult;
            public int ShowCount = 0;
#if !SILVERLIGHT
            public MessageBoxImage Icon;
            public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult) {
                MessageBoxTest = messageBoxText;
                Caption = caption;
                Button = button;
                Icon = icon;
                DefaultResult = defaultResult;
                ShowCount++;
                return Result;
            }
#else
            public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) {
                MessageBoxTest = messageBoxText;
                Caption = caption;
                Button = button;
                DefaultResult = defaultResult;
                ShowCount++;
                return Result;
            }
#endif

        }

#if !SILVERLIGHT
        MessageBoxService CreateMessageService() {
            return new MessageBoxService();
        }
#else
        MessageBoxService CreateMessageService() {
            return new MessageBoxService();
        }
#endif
        [Test]
        public void GetActualServiceTest1() {
            var b = new ConfirmationBehavior();
            var service = CreateMessageService();
            b.MessageBoxService = service;
            Assert.AreEqual(service, b.GetActualService());
        }
        [Test]
        public void GetActualServiceTest2() {
            UserControl root = new UserControl();
            var service = CreateMessageService();
            Interaction.GetBehaviors(root).Add(service);
            var viewModel = new TestViewModelBase();
            root.DataContext = viewModel;
            Button button = new Button();
            var b = new ConfirmationBehavior();
            Interaction.GetBehaviors(button).Add(b);
            root.Content = button;
            Assert.AreEqual(service, viewModel.MessageBoxService);
            Assert.AreEqual(service, b.GetActualService());
        }
        [Test]
        public void GetActualServiceTest3() {
            UserControl root = new UserControl();
            var viewModel = new TestViewModelBase();
            root.DataContext = viewModel;
            Button button = new Button();
            var b = new ConfirmationBehavior();
            Interaction.GetBehaviors(button).Add(b);
            root.Content = button;
            var service = b.GetActualService();
            var service2 = b.GetActualService();
            var service3 = Interaction.GetBehaviors(button).First(x => x is IMessageBoxService);
            Assert.AreEqual(service, service2);
            Assert.AreEqual(service, service3);
        }
        [Test]
        public void GetActualServiceTest4() {
            UserControl root = new UserControl();
            var service = CreateMessageService();
            Interaction.GetBehaviors(root).Add(service);
            Button button = new Button();
            var b = new ConfirmationBehavior();
            Interaction.GetBehaviors(button).Add(b);
            root.DataContext = new object();
            root.Content = button;
            var viewModel = new TestViewModelBase();
            button.DataContext = viewModel;
            var service2 = b.GetActualService();
            var service3 = b.GetActualService();
            Assert.IsNotNull(viewModel.MessageBoxService);
            Assert.AreNotEqual(service, service2);
            Assert.AreEqual(service2, service3);
            b.MessageBoxService = service;
            Assert.AreEqual(service, b.GetActualService());
        }

        [Test]
        public void SetAssociatedObjectCommandPropertyTest1() {
            Button control = new Button();
            var b = new ConfirmationBehavior();
            Interaction.GetBehaviors(control).Add(b);
            var command = new DelegateCommand(() => { });
            var res = b.SetAssociatedObjectCommandProperty(command);
            Assert.IsTrue(res);
            Assert.AreEqual(command, control.Command);
        }
        [Test]
        public void SetAssociatedObjectCommandPropertyTest3() {
            TestControl1 control = new TestControl1();
            var b = new ConfirmationBehavior();
            Interaction.GetBehaviors(control).Add(b);
            var command = new DelegateCommand(() => { });
            var res = b.SetAssociatedObjectCommandProperty(command);
            Assert.IsTrue(res);
            Assert.AreEqual(command, control.Command);
        }

        [Test]
        public void CanExecuteChangedTest1() {
            int canExecute = 0;
            int confirmationCanExecute = 0;
            DelegateCommand command = new DelegateCommand(
                () => { },
                () => {
                    canExecute++;
                    return true;
                }
            );
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.ConfirmationCommand.CanExecuteChanged += (d, e) => {
                confirmationCanExecute++;
            };
            b.Command = command;
            Assert.AreEqual(0, canExecute);
            Assert.AreEqual(1, confirmationCanExecute);
            Button control = new Button();
            Interaction.GetBehaviors(control).Add(b);
            Assert.AreEqual(1, canExecute);
            Assert.AreEqual(1, confirmationCanExecute);
            Assert.AreEqual(b.ConfirmationCommand, control.Command);
        }
        [Test]
        public void CanExecuteChangedTest2() {
            Button control = new Button();
            bool isCommandEnabled = true;
            DelegateCommand command = DelegateCommandFactory.Create(
                () => { },
                () => {
                    return isCommandEnabled;
                }, false);
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.Command = command;
            Interaction.GetBehaviors(control).Add(b);
            Assert.IsTrue(control.IsEnabled);
            isCommandEnabled = false;
            command.RaiseCanExecuteChanged();
            Assert.IsFalse(control.IsEnabled);
        }

        [Test]
        public void ExecuteAndCommandParameterTest() {
            var service = new TestMessageBoxService();
            object executeCommandParameter = null;
            object canExecuteCommandParameter = null;
            DelegateCommand<object> command = new DelegateCommand<object>(
                x => {
                    executeCommandParameter = x;
                }, x => {
                    canExecuteCommandParameter = x;
                    return true;
                });
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.MessageBoxService = service;
            Button control = new Button();
            Interaction.GetBehaviors(control).Add(b);
            b.Command = command;
            object controlCommandParameter = new object();
            control.CommandParameter = controlCommandParameter;
            Assert.IsNull(executeCommandParameter);
            control.Command.Execute(controlCommandParameter);
            Assert.AreEqual(controlCommandParameter, executeCommandParameter);
            Assert.AreEqual(controlCommandParameter, canExecuteCommandParameter);
            object confirmationBehaviorCommandParameter = new object();
            b.CommandParameter = confirmationBehaviorCommandParameter;
            Assert.AreEqual(controlCommandParameter, executeCommandParameter);
            Assert.AreEqual(confirmationBehaviorCommandParameter, canExecuteCommandParameter);
            control.Command.Execute(controlCommandParameter);
            Assert.AreEqual(confirmationBehaviorCommandParameter, executeCommandParameter);
            Assert.AreEqual(confirmationBehaviorCommandParameter, canExecuteCommandParameter);
        }
        [Test]
        public void ExecuteAndMessageBoxServiceTest() {
            int executeCount = 0;
            DelegateCommand command = new DelegateCommand(() => executeCount++, () => true);
            TestMessageBoxService service = new TestMessageBoxService();
            Button control = new Button();
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.Command = command;
            Interaction.GetBehaviors(control).Add(b);
            b.MessageBoxService = service;
            control.Command.Execute(null);
            Assert.AreEqual(1, executeCount);
            Assert.AreEqual(1, service.ShowCount);
            Assert.AreEqual("Confirmation", service.Caption);
            Assert.AreEqual("Do you want to perform this action?", service.MessageBoxTest);
#if !SILVERLIGHT
            Assert.AreEqual(MessageBoxImage.None, service.Icon);
            Assert.AreEqual(MessageBoxButton.YesNo, service.Button);
#else
            Assert.AreEqual(MessageBoxButton.OKCancel, service.Button);
#endif
            Assert.AreEqual(MessageBoxResult.None, service.DefaultResult);

            b.MessageText = "MessageText";
            b.MessageTitle = "MessageTitle";
#if !SILVERLIGHT
            b.MessageIcon = MessageBoxImage.Hand;
#endif
            b.MessageButton = MessageBoxButton.OKCancel;
            b.MessageDefaultResult = MessageBoxResult.Cancel;
            service.Result = MessageBoxResult.OK;
            control.Command.Execute(null);
            Assert.AreEqual(2, executeCount);
            Assert.AreEqual(2, service.ShowCount);
            Assert.AreEqual("MessageTitle", service.Caption);
            Assert.AreEqual("MessageText", service.MessageBoxTest);
#if !SILVERLIGHT
            Assert.AreEqual(MessageBoxImage.Hand, service.Icon);
#endif
            Assert.AreEqual(MessageBoxButton.OKCancel, service.Button);
            Assert.AreEqual(MessageBoxResult.Cancel, service.DefaultResult);

            service.Result = MessageBoxResult.Cancel;
            control.Command.Execute(null);
            Assert.AreEqual(2, executeCount);
            Assert.AreEqual(3, service.ShowCount);
        }
    }
}