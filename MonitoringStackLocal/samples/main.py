import logging
import time
from extensions.opentelemetry import configure_opentelemetry

logger = logging.getLogger(__name__)

configure_opentelemetry(
    enable_otel=True
)

for _ in range(100):

    logger.info("OpenTelemetry configured successfully.")
    logger.info("This is an informational message")
    logger.warning("This is a warning message")
    logger.error("This is an error message")
    logger.debug("This is a debug message")
    logger.critical("This is a critical message")
    logger.exception("This is an exception message")

    time.sleep(1)
