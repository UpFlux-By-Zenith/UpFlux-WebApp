export const formatTimestamp = (timestamp: string): string => {

    if (timestamp === "") return ""
    const date = new Date(timestamp);

    // Format options
    const options: Intl.DateTimeFormatOptions = {
        year: "numeric",
        month: "long",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
        hour12: true,
    };

    return date.toLocaleString("en-US", options);
};

