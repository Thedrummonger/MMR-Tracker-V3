from abc import ABC, abstractmethod
from MIscData import *

class CheckableLocation(ABC):
    def __init__(self, parent):
        self._parent = parent
        self.ID = None
        self.Available = False
        self.CheckState = MiscData.CheckState.Unchecked
        self.Starred = False
        self.Hidden = False
        self.Price = None
        self.Currency = None
        self.DisplayName = None
        self.RandomizedState = MiscData.RandomizedState.Randomized
        self.referenceData = InstanceData.ReferenceData()

    def GetParent(self):
        return self._parent

    def SetParent(self, parent):
        self._parent = parent

    @abstractmethod
    def GetName(self):
        pass

    @abstractmethod
    def GetAbstractDictEntry(self):
        pass

    @abstractmethod
    def LocationType(self):
        pass

    @abstractmethod
    def GetPrice(self):
        pass

    @abstractmethod
    def SetPrice(self, inPrice, inCurrency='\0'):
        pass

    def hasPrice(self):
        price, _ = self.GetPrice()
        return price is not None and price >= 0

    def GetPrice(self):
        return self.Price if self.Price is not None else -1, self.Currency if self.Currency is not None else '$'
