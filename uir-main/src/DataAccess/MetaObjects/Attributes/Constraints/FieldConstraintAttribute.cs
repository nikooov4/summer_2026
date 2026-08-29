using System;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    [Serializable]
    public abstract class FieldConstraintAttribute : ConstraintAttribute
    {
        /// <summary>
        /// Проверка того, что новое значение будет удовлетворять ограничению
        /// </summary>
        /// <param name="entity">объект, для которого выполняется проверка</param>
        /// <param name="property">свойство</param>
        /// <param name="value">новое, еще не присвоенное значение, допустимость которого требуется проверить</param>
        /// <returns></returns>
        public abstract bool CheckValue(IEntity entity, EntityAttributeDescription property, object value);

        /// <summary>
        /// Проверка того, что установленное значение свойства удовлетворяет ограничению
        /// </summary>
        /// <param name="entity">Объект, для которого необходимо выполнить проверку</param>
        /// <param name="property">Свойтво, допустимость значения которого необходимо проверить</param>
        /// <returns></returns>
        public abstract bool CheckValue(EntityBase entity, EntityAttributeDescription property);
    }
}