import logging
import time
from extensions.opentelemetry import configure_opentelemetry
from extensions.configuration import set_defaults

from opentelemetry.metrics import get_meter
from opentelemetry.trace import get_tracer

set_defaults(
        system='TestSystem',
        application='TestApplication',
        deployable_unit='TestDeployableUnit',
        version='0.0.1'
)
logger = logging.getLogger(__name__)

configure_opentelemetry(
    enable_otel=True
)

meter = get_meter(__name__)
# Create a tracer
tracer = get_tracer(__name__)
# Create a counter
request_counter = meter.create_counter(
    name="app.request.count",
    description="Total number of requests",
    unit="1"
)


for _ in range(100):
    request_counter.add(1, {"route": "/home"})

    with tracer.start_as_current_span("main"):
        logger.info("OpenTelemetry configured successfully.")
        logger.info("This is an informational message")
        logger.warning("This is a warning message")
        logger.error("This is an error message")
        logger.debug("This is a debug message")
        logger.critical("This is a critical message")
        logger.exception("This is an exception message")

    time.sleep(1)
