function [A, B, C] = sinefit(angle, speed, offset)

    %% pre-process
    % takes a number or array of numbers "angle". If they are outside the
    % limits ("min" and "max") they are wrapped to fit into the limits
    max = 360;
    min = 0;
    period = max - min;

    angle = (angle + offset);
    
    i = 1;
    while i <= length(angle) 
        if angle(i) <= min
            angle(i) = period + angle(i);
        elseif angle(i) > max
            angle(i) = angle(i) - period;
        end
        i = i + 1;
    end 

    angle = angle * pi / 180;

    %% fits a sinasoid with frequency 2*pi through the (x,y) data and returns a
    % Coef A, B, C that defines that sinasoid as follows:
    %       A * cos(angle) + B * sin(angle) + C

    fit = @(b, angle) b(1).*cos(angle) + b(2).*sin(angle) + b(3); %fitting function
    fcn = @(b) sum((fit(b, angle) - speed).^2);                 %least-squares cost function
    s = fminsearch(fcn, [0, 0, mean(speed)]);              %minimize least-squares
    A = s(1);
    B = s(2);
    C = s(3);
end
