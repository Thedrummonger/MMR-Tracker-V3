from enum import Enum
from typing import List, Dict, Tuple, Union, Callable, Iterable
import functools


class MiscData:

    class CheckState(Enum):
        Checked = 0
        Marked = 1
        Unchecked = 2

    class UnrandState(Enum):
        Any = 0
        Unrand = 1
        Manual = 2

    class LogicFileType(Enum):
        Logic = 0
        Additional = 1
        Runtime = 2

    class DebugMode(Enum):
        Debugging = 1
        UserView = 2
        Off = 3

    class MathOP(Enum):
        add = "add"
        subtract = "subtract"
        multiply = "multiply"
        divide = "divide"
        set = "set"

    class DisplayListType(Enum):
        Locations = 0
        Checked = 1
        Entrances = 2

    class UILayout(Enum):
        Vertical = 0
        Horizontal = 1
        Compact = 2

    class JSONType(Enum):
        Newtonsoft = 0
        UTF8 = 1
        DotNet = 2

    class RandomizedState(Enum):
        Rand = 0
        UnRand = 1
        Manual = 2
        Junk = 3

    class TimeOfDay(Enum):
        none = 0
        Day1 = 1
        Night1 = 2
        Day2 = 4
        Night2 = 8
        Day3 = 16
        Night3 = 32

    class CheckableLocationTypes(Enum):
        location = 0
        Exit = 1
        Hint = 2
        macro = 3

    @staticmethod
    def ThrowNotImplementedException(message):
        raise NotImplementedError(message)