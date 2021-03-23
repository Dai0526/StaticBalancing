classdef CWstateclass
    %UNTITLED7 Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        count
        stackheight
        mass
        activeradius
        CGradius
        appliedimbalance
    end
    
    methods
        function obj = CWstateclass(position,weight,count)
            if length(position)<2
                 error('Bad "position" field')
             end
             for i=1:length(position)
                 if ~strcmp(class(position(i)),'balpositionclass')
                     error('Bad "position" field')
                 end
             end
             for i=1:length(weight)
                 if ~strcmp(class(weight(i)),'counterweightclass')
                    error('Bad "weight" field')
                 end
             end
             if ~isequal(size(count),[length(position), length(weight)]) || ~isnumeric(count)
                 error('Bad "count" field')
             end
             
             stackheight = sum((ones(2,1)*[weight(:).thickness]).*count,2);
             mass = sum((ones(2,1)*[weight(:).mass]).*count,2);
             
             for i=1:length(position)
                 switch position(i).stackdirection
                     case 'Axial'
                         activeradius(i,:) = position(i).radius;
                         CGradius(i,:) = position(i).radius;
                     case 'Radial+'
                         activeradius(i,:) = position(i).radius + stackheight(i);
                         CGradius(i,:) = position(i).radius + stackheight(i)/2;
                     case 'Radial-'
                         activeradius(i,:) = position(i).radius - stackheight(i);
                         CGradius(i,:) = position(i).radius - stackheight(i)/2;
                     otherwise
                         error('invalid "stackdirection" field');
                 end
             end
             appliedimbalance = mass.*CGradius;
             
             obj.count = count;
             obj.stackheight = stackheight;
             obj.mass = mass;
             obj.activeradius = activeradius;
             obj.CGradius = CGradius;
             obj.appliedimbalance = appliedimbalance;
        end
    end
    
end

