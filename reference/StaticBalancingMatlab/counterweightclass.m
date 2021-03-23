classdef counterweightclass
    %UNTITLED3 Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        name
        mass
        thickness
    end
    
    methods
        function obj = counterweightclass(name,mass,thickness)
           if isempty(name) || ~isa(name,'char')
               error('Bad "name" field')
           end
           if isempty(mass) || ~isscalar(mass) || ~isnumeric(mass)
               error('Bad "radius" field')
           end
           if isempty(thickness) || ~isscalar(thickness) || ~isnumeric(thickness)
               error('Bad "angle" field')
           end

           obj.name = name;
           obj.mass = mass;
           obj.thickness = thickness;         
        end
    end
    
end

