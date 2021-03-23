function angle = constrainangle(angle,min,max)
% takes a number or array of numbers "angle". If they are outside the
% limits ("min" and "max") they are wrapped to fit into the limits
    period = max-min;
    i=1;
    while i<=length(angle) 
        if angle(i)<=min
            angle(i) = period+angle(i);
            continue
        elseif angle(i)>max
            angle(i) = angle(i)-period;
            continue
        else
            i=i+1;
        end
    end            
end