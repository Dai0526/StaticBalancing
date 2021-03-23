classdef balpositionclass
    %UNTITLED2 Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        name
        radius
        angle
        maxstackheight
        stackdirection
    end
    
    methods
        function obj = balpositionclass(name,radius,angle,maxstackheight,stackdirection)
           if isempty(name) || ~isa(name,'char')
               error('Bad "name" field')
           end
           if isempty(radius) || ~isscalar(radius) || ~isnumeric(radius)
               error('Bad "radius" field')
           end
           if isempty(angle) || ~isscalar(angle) || ~isnumeric(angle)
               error('Bad "angle" field')
           end
           if isempty(maxstackheight) || ~isscalar(maxstackheight) || ~isnumeric(maxstackheight)
               error('Bad "maxstackheight" field')
           end
           switch stackdirection
               case {'r+', 'Radial+'}
                   obj.stackdirection = 'Radial+';
               case {'r-', 'Radial-'}
                   obj.stackdirection = 'Radial-';
               case {'a', 'Axial'}
                   obj.stackdirection = 'Axial';
               otherwise
                   error('Bad "stackdirection" field')
           end

           obj.name = name;
           obj.radius = radius;
           obj.angle = angle;
           obj.maxstackheight = maxstackheight;          
        end
        
        function pass = CheckStackHeight(obj,stackheight)
            if isempty(stackheight) || ~isscalar(stackheight) || ~isnumeric(stackheight)
                error('Bad "stackheight" parameter')
            end
            
            pass = ~(obj.maxstackheight < stackheight);
        end
    end
end
