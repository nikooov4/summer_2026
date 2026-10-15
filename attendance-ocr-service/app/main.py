from __future__ import annotations

from fastapi import FastAPI
from fastapi.staticfiles import StaticFiles
from fastapi.middleware.cors import CORSMiddleware

from .dependencies import AppContainer, container, get_container
from .routers import demo, health, images, jobs, review


def create_app(app_container: AppContainer = container) -> FastAPI:
    app = FastAPI(
        title=app_container.settings.app_title,
        version=app_container.settings.app_version,
    )
    app.dependency_overrides[get_container] = lambda: app_container
    app.add_middleware(
        CORSMiddleware,
        allow_origins=list(app_container.settings.allowed_origins),
        allow_methods=["GET", "POST", "PATCH"],
        allow_headers=["Content-Type"],
    )
    app.mount("/static", StaticFiles(directory=app_container.settings.static_dir), name="static")
    app.include_router(health.router)
    app.include_router(images.router)
    app.include_router(jobs.router)
    app.include_router(review.router)
    app.include_router(demo.router)
    return app


app = create_app()
