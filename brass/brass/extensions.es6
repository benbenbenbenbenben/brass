// 'use strict'

// prototypes
if (Array.prototype.flatten == undefined) {
    Array.prototype.flatten = function() {
        return (arr => arr.reduce((acc, val) => acc.concat(Array.isArray(val) ? val.flatten() : val), []))(this);
    }; 
}
if (Window.prototype.findText == undefined) {
    Window.prototype.findTextInElement = (pattern, parent) =>
        [(parent.outerHTML.indexOf(pattern) > 0) ? parent : null]
        .concat(Array.from(parent.children).map(n => window.findTextInElement(pattern, n)))
        .flatten()
        .filter(x => x != null);
    Window.prototype.findText = pattern => window.findTextInElement(pattern, document.body);
}



