/*  Copyright 2016 Clive Pottinger
    This file is part of the WalkAbout Mod.

    WalkAbout is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    WalkAbout is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with WalkAbout.  If not, see<http://www.gnu.org/licenses/>.
*/
using System;

namespace KspWalkAbout.Values
{
    [Flags]
    public enum FacilityLevels
    {
        None = 0x00,
        Level_1 = 0x01,
        Level_2 = 0x02,
        Levels_1_2 = 0x03,
        Level_3 = 0x04,
        Levels_1_3 = 0x05,
        Levels_2_3 = 0x06,
        Levels_1_2_3 = 0x07,
    }

    public enum GuiState { force, offNotActive, offPauseOpen, displayed };
}
