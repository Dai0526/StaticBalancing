function [magnitude, direction] = magdir(v)
% given a 2D vector in a standard (XY) vectorspace, this function returns
% the mangitude and direction (in deg) of that vector

magnitude = norm(v);
direction = atan2(v(2),v(1))*180/pi;