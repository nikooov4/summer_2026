from __future__ import annotations

from collections import deque
from typing import Protocol


class JobQueue(Protocol):
    def enqueue(self, job_id: str) -> None:
        """Add a job command to the processing queue."""


class InMemoryJobQueue:
    def __init__(self) -> None:
        self._items: deque[str] = deque()

    def enqueue(self, job_id: str) -> None:
        self._items.append(job_id)

    def dequeue(self) -> str | None:
        if not self._items:
            return None
        return self._items.popleft()

    def remove(self, job_id: str) -> None:
        self._items = deque(item for item in self._items if item != job_id)

    def pending(self) -> list[str]:
        return list(self._items)

    def clear(self) -> None:
        self._items.clear()
