// 'use strict'

find = function(pattern, parent) {
    return [(parent.outerHTML.indexOf(pattern) > 0) ? parent : null]
        .concat(Array.from(parent.children).map(function(n) {
            return find(pattern, n);
        }));
};
var flatten = arr => arr.reduce((acc, val) => acc.concat(Array.isArray(val) ? flatten(val) : val), []);