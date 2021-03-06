#region License

// MIT License
// 
// Copyright (c) 2018 
// Marcus Technical Services, Inc.
// http://www.marcusts.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

namespace SharedForms.Views.SubViews
{
   #region Imports

   using System;
   using Autofac;
   using Common.Interfaces;
   using Common.Navigation;
   using Common.Notifications;
   using Common.Utils;
   using Pages;
   using SharedGlobals.Container;
   using Xamarin.Forms;

   #endregion

   public interface IMainMenu
   {
      bool IsMenuLoaded { get; }

      double MenuHeight { get; }
   }

   public class MainMenu : ContentView, IMainMenu
   {
      private const bool ALLOW_EVENT_TUNNELING = false;

      private static readonly double MENU_ITEM_WIDTH = 120.0;
      private static readonly double MENU_OUTSIDE_SINGLE_MARGIN = 15.0;

      public static readonly double MENU_GROSS_WIDTH = MENU_ITEM_WIDTH + 2 * MENU_OUTSIDE_SINGLE_MARGIN;

      private static readonly Thickness MENU_OUTSIDE_MARGIN = new Thickness(MENU_OUTSIDE_SINGLE_MARGIN);
      private static readonly double MENU_INSIDE_SINGLE_MARGIN = MENU_OUTSIDE_SINGLE_MARGIN / 2;
      private static readonly double MENU_ITEM_HEIGHT = 40.0;
      private static readonly double MAIN_MENU_OPACITY = 0.95;
      private readonly IStateMachineBase _stateMachine;
      private readonly IFormsMessenger _formsMessenger;
      private bool _isMenuLoaded;

      public MainMenu()
      {
         using (var scope = AppContainer.GlobalVariableContainer.BeginLifetimeScope())
         {
            _stateMachine = scope.Resolve<IStateMachineBase>();
            _formsMessenger = scope.Resolve<IFormsMessenger>();
         }

         // Not really used
         BindingContext = this;

         VerticalOptions = LayoutOptions.StartAndExpand;
         HorizontalOptions = LayoutOptions.CenterAndExpand;

         BackgroundColor = Colors.HEADER_AND_TOOLBAR_COLOR;
         Opacity = MAIN_MENU_OPACITY;

         InputTransparent = ALLOW_EVENT_TUNNELING;

         LoadMenuFromStateMachine();
      }

      public bool IsMenuLoaded
      {
         get => _isMenuLoaded;
         private set
         {
            _isMenuLoaded = value; 
            
            _formsMessenger.Send(new MenuLoadedMessage());
         }
      }

      public double MenuHeight { get; private set; }

      private void LoadMenuFromStateMachine()
      {
         // A grid to handle the entire menu
         var menuStack = FormsUtils.GetExpandingStackLayout();
         menuStack.VerticalOptions = LayoutOptions.StartAndExpand;
         menuStack.HorizontalOptions = LayoutOptions.CenterAndExpand;
         menuStack.Margin = MENU_OUTSIDE_MARGIN;
         menuStack.Spacing = MENU_INSIDE_SINGLE_MARGIN;
         menuStack.InputTransparent = ALLOW_EVENT_TUNNELING;

         var singleMenuItemHeight = MENU_ITEM_HEIGHT + MENU_INSIDE_SINGLE_MARGIN;

         // Allow for the top and bottom margins, etc.
         MenuHeight = 2 * MENU_OUTSIDE_SINGLE_MARGIN;

         foreach (var menuData in _stateMachine.MenuItems)
         {
            menuStack.Children.Add(CreateMenuItemButton(menuData));
            MenuHeight += singleMenuItemHeight;
         }

         HeightRequest = MenuHeight;
         WidthRequest = MENU_GROSS_WIDTH;

         var scroller = FormsUtils.GetExpandingScrollView();
         scroller.InputTransparent = ALLOW_EVENT_TUNNELING;
         scroller.Content = menuStack;

         Content = scroller;

         IsMenuLoaded = true;
      }

      private Button CreateMenuItemButton(IMenuNavigationState menuData)
      {
         var retButton = 
            new Button
               {
                  Text = menuData.MenuTitle,
                  WidthRequest = MENU_ITEM_WIDTH,
                  HeightRequest = MENU_ITEM_HEIGHT,
                  HorizontalOptions = LayoutOptions.Center,
                  VerticalOptions = LayoutOptions.Center,
                  InputTransparent = ALLOW_EVENT_TUNNELING
            };

         retButton.Clicked += (s, e) =>
            {
               // Ask to close the menu as if the user tapped the hamburger icon.
               _formsMessenger.Send(new NavBarMenuTappedMessage());

               _stateMachine.GoToAppState<IMenuNavigationState>(menuData.AppState, menuData);
            };

         return retButton;
      }
   }
}
