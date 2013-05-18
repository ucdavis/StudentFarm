/*
 * Timespinner based on JQuery UI's spinner. Adds up/down arrow buttons to a textbox
 * for increasing/decreasing time of day by five minutes. Also allows increasing
 * decreasing the time by an hour by using page up/page down buttons and by five
 * minutes by using keyboard arrow buttons.
 */

// Converts a time written in minutes since midnight to 12-hour time (e.g., 12:00 AM)
// or 24-hour time.
function formatTime(time, type) {
    if (time < 0) {
        time = 1440 + time;
    }
    var normalizedTime = time % 1440;
    var minutes = normalizedTime % 60;
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var hour = Math.floor(normalizedTime / 60);
    var amPm = '';
    if (type == 12) {
        amPm = hour < 12 ? ' AM' : ' PM';
        if (hour > 12) { hour = hour - 12; }
        if (hour == '00') { hour = 12; }
    }
    return hour + ':' + minutes + amPm;
}

// Converts a time written in 24-hour or 12-hour time to minutes since midnight,
// assuming 24-hour time is in the format 00:00 and 12-hour time is in the format
// 12:00 AM.
function parseTime(val) {
    if (typeof val === 'string' && val != "") {
        if (val.indexOf('AM') != -1) {
            var hour = Number(val.split(':')[0]);
            if (hour == 12) { hour = 0; }
            var minute = Number(val.split(':')[1].split(' ')[0]);
            return (hour * 60) + minute;
        }
        else if (val.indexOf('PM') != -1) {
            var hour = Number(val.split(':')[0]);
            if (hour != 12) { hour = hour + 12; }
            var minute = Number(val.split(':')[1].split(' ')[0]);
            return (hour * 60) + minute;
        }
            // If they entered a time without putting in AM or PM, assume it's
            // in 24-hour time.
        else if (val.indexOf(':') != -1) {
            try {
                var hour = Number(val.split(':')[0]);
                var minute = Number(val.split(':')[1]);
                if (!isNaN(hour) && !isNaN(minute)) {
                    return (hour * 60) + minute;
                }
            }
            catch (e) {
                console.log('Invalid time format');
            }
        }
    }
    else if (Number(val) == val) {
        return val;
    }
    return null;
}

// Set up timespinner widget based on spinner widget.
jQuery.widget('ui.timespinner', jQuery.ui.spinner, {
    options: {
        page: 12, // page up/page down buttons do 12 steps (12 steps * 5 minutes/step = 60 minutes = 1 hour)
        step: 5   // arrow buttons on keyboard and on spinner do 5 minutes
    },
    _parse: function (val) {
        return parseTime(val);
    },
    _format: function (val) {
        return formatTime(val, 12);
    },
});