using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMKit.Maestro.UI;

public static class UIConstants
{
    public const double ChatMessageMaximumWidth = 800;

    public const double ChatWindowLayoutMinimumWidth = 720;

    public const double WindowMinimumWidth = 568;

    public const double WindowMinimumHeight = 600;

    public const double PopupWidth = 536;

    public const double AlertPopupWidth = 400;

    public const double ModelSelectionButtonMaxWidth = 568;

    public const double ChatPageSidebarWidth = 300;

#if WINDOWS || NET9_0
    public const double TabBarHeight = 48;

    public const double PageTopBarHeight = 64;

    public const double MinimizedHeaderButtonWidth = ChatWindowLayoutMinimumWidth - (16 * 2);

    public const double ModelSelectionButtonHeight = 48;

    public const double HeaderHorizontalMargin = 12;

    public const double ChatPageSideTogglesWidth = (ChatPageToggleButtonWidth * 2) + 16 + 8;

    public const double ChatPageToggleButtonWidth = 32;
#elif MACCATALYST
    public const double TabBarHeight = 48;

    public const double PageTopBarHeight = 64;

    public const double MinimizedHeaderButtonWidth = ChatWindowLayoutMinimumWidth - (16 * 2);

    public const double ModelSelectionButtonHeight = 48;

    public const double HeaderHorizontalMargin = 12;

    public const double ChatPageSideTogglesWidth = (ChatPageToggleButtonWidth * 2) + 16 + 8;

    public const double ChatPageToggleButtonWidth = 32;
#endif
}
