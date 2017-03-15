// 'use strict'

// prototypes
"use strict";

if (Array.prototype.flatten == undefined) {
    Array.prototype.flatten = function () {
        return (function (arr) {
            return arr.reduce(function (acc, val) {
                return acc.concat(Array.isArray(val) ? val.flatten() : val);
            }, []);
        })(this);
    };
}
if (Window.prototype.findText == undefined) {
    Window.prototype.findTextInElement = function (pattern, parent) {
        return [parent.outerHTML.indexOf(pattern) > 0 ? parent : null].concat(Array.from(parent.children).map(function (n) {
            return window.findTextInElement(pattern, n);
        })).flatten().filter(function (x) {
            return x != null;
        });
    };
    Window.prototype.findText = function (pattern) {
        return window.findTextInElement(pattern, document.body);
    };
}

